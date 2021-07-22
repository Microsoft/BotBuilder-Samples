# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

import json
import os
from typing import List

from botbuilder.core import (
    BotAdapter,
    ExtendedUserTokenProvider,
    MessageFactory,
    TurnContext,
)
from botbuilder.schema import ActionTypes, Attachment, CardAction
from botbuilder.schema.teams import (
    TaskModuleRequest,
    TaskModuleResponse,
    TaskModuleTaskInfo,
    TabRequest,
    TabResponsePayload,
    TabResponse,
    TabResponseCard,
    TabResponseCards,
    TabSubmit,
    TabSuggestedActions,
)
from botbuilder.core.teams import TeamsActivityHandler

from config import DefaultConfig
from models import (
    CardResources,
    UIConstants,
    TaskModuleResponseFactory,
)

from simple_graph_client import SimpleGraphClient


class TeamsAdaptiveCardTabsBot(TeamsActivityHandler):
    def __init__(self, config: DefaultConfig):
        self.__base_url = config.BASE_URL
        self._connection_name = config.CONNECTION_NAME

    async def on_message_activity(self, turn_context: TurnContext):
        """
        See https:#aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        """

        activity_text = (
            turn_context.activity.text.strip().lower()
            if turn_context.activity and turn_context.activity.text
            else None
        )
        if activity_text == "logout":
            if not isinstance(turn_context.adapter, ExtendedUserTokenProvider):
                raise Exception("logout: not supported by the current adapter")

            await turn_context.adapter.sign_out_user(
                turn_context, self._connection_name
            )
            reply = MessageFactory.text("You have been signed out.")
            await turn_context.send_activity(reply)
        else:
            reply = MessageFactory.text(
                "Hello, I am a Teams Adaptive Card Tabs bot."
                "  Please use the Tabs to interact. (Send 'logout' to sign out)"
            )
            await turn_context.send_activity(reply)

    async def on_teams_tab_fetch(
        self, turn_context: TurnContext, tab_request: TabRequest
    ):
        tab_entity_id = (
            tab_request.tab_entity_context.tab_entity_id
            if tab_request and tab_request.tab_entity_context
            else None
        )
        if tab_entity_id == "workday":
            magic_code = ""
            if tab_request.state:
                magic_code = str(tab_request.state)

            return await self.get_primary_tab_response(turn_context, magic_code)
        else:
            return self._get_tab_response(
                [
                    CardResources.WELCOME,
                    CardResources.INTERVIEW_CANDIDATES,
                    CardResources.VIDEO_ID,
                ]
            )

    async def on_teams_tab_submit(
        self, turn_context: TurnContext, tab_submit: TabSubmit
    ):
        tab_entity_id = (
            tab_submit.tab_entity_context.tab_entity_id
            if tab_submit and tab_submit.tab_entity_context
            else None
        )
        if tab_entity_id == "workday":
            data = (
                turn_context.activity.value["data"]
                if turn_context.activity
                and turn_context.activity.value
                and "data" in turn_context.activity.value
                else None
            )
            if data.get("shouldLogout"):
                credentials = self._get_credentials(turn_context)

                await turn_context.adapter.sign_out_user(
                    turn_context,
                    self._connection_name,
                    turn_context.activity.from_property.id,
                    credentials,
                )
                return self._get_tab_response([CardResources.SUCCESS])

            response = await self.get_primary_tab_response(turn_context, None)
            # If the user is not signed in, the .Tab type will be auth.
            success_card = self._create_adaptive_card_attachment(CardResources.SUCCESS)
            response.tab.value.cards.insert(0, success_card)

            return response
        else:
            return self._get_tab_response(
                [
                    CardResources.WELCOME,
                    CardResources.INTERVIEW_CANDIDATES,
                    CardResources.VIDEO_ID,
                ]
            )

    async def on_teams_task_module_fetch(
        self, turn_context: TurnContext, task_module_request: TaskModuleRequest
    ) -> TaskModuleResponse:
        video_id = (
            task_module_request.data.get("youTubeVideoId")
            if task_module_request.data
            else None
        )
        video_id = str(video_id) if video_id else None
        task_info = TaskModuleTaskInfo(
            height=UIConstants.YOUTUBE.height,
            width=UIConstants.YOUTUBE.width,
            title=str(UIConstants.YOUTUBE.title),
        )
        if video_id:
            task_info.url = (
                task_info.fallback_url
            ) = f"https://www.youtube.com/embed/{ video_id }"
        else:
            # No video ID is present, so return the InputText card.
            attachment = Attachment(
                content_type="application/vnd.microsoft.card.adaptive",
                content=self._create_adaptive_card_attachment(
                    CardResources.INPUT_TEXT
                ).card,
            )
            task_info.height = UIConstants.ADAPTIVE_CARD.height
            task_info.width = UIConstants.ADAPTIVE_CARD.width
            task_info.card = attachment

        return TaskModuleResponseFactory.to_task_module_response(task_info)

    async def on_teams_task_module_submit(
        self, turn_context: TurnContext, task_module_request: TaskModuleRequest
    ) -> TaskModuleResponse:
        return TaskModuleResponseFactory.create_response("Thanks!")

    async def get_primary_tab_response(
        self, turn_context: TurnContext, magic_code: str
    ):
        # pylint: disable=unnecessary-lambda
        credentials = self._get_credentials(turn_context)
        token = await turn_context.adapter.get_user_token(
            turn_context, self._connection_name, magic_code, credentials
        )
        # If a token is returned, the user is logged in.
        str_token = token.token if token else ""
        if str_token:
            cards = list(
                map(
                    lambda card: self._create_adaptive_card_attachment(card),
                    [CardResources.QUICK_ACTIONS, CardResources.ADMIN_CONFIG],
                )
            )

            # Add the manager card.
            client = SimpleGraphClient(str_token)
            user = await client.get_me()
            display_name = user.get("displayName", "[unknown]") if user else "[unknown]"

            manager_card = self._create_adaptive_card_attachment(
                CardResources.MANAGER_DASHBOARD, "[profileName]", display_name
            )
            cards.insert(1, manager_card)

            return TabResponse(
                tab=TabResponsePayload(
                    type="continue", value=TabResponseCards(cards=cards)
                )
            )

        # The user is not logged in, so send an "auth" response.
        sign_in_resource = (
            await turn_context.adapter.get_sign_in_resource_from_user_and_credentials(
                turn_context,
                credentials,
                self._connection_name,
                turn_context.activity.from_property.id,
            )
        )
        return TabResponse(
            tab=TabResponsePayload(
                type="auth",
                suggested_actions=TabSuggestedActions(
                    actions=[
                        CardAction(
                            title="login",
                            type=ActionTypes.open_url,
                            value=sign_in_resource.sign_in_link,
                        )
                    ]
                ),
            )
        )

    @staticmethod
    def _get_tab_response(card_types: List[str]) -> TabResponse:
        # pylint: disable=unnecessary-lambda
        cards = list(
            map(
                lambda card: TeamsAdaptiveCardTabsBot._create_adaptive_card_attachment(
                    card
                ),
                card_types,
            )
        )

        return TabResponse(
            tab=TabResponsePayload(type="continue", value=TabResponseCards(cards=cards))
        )

    @staticmethod
    def _get_credentials(turn_context):
        connector = turn_context.turn_state.get(BotAdapter.BOT_CONNECTOR_CLIENT_KEY)
        credentials = (
            connector.config.credentials
            if connector
            and getattr(connector, "config", None)
            and getattr(connector.config, "credentials", None)
            else None
        )
        if not (
            isinstance(turn_context.adapter, ExtendedUserTokenProvider) and credentials
        ):
            raise Exception("The current adapter does not support OAuth.")

        return credentials

    @staticmethod
    def _create_adaptive_card_attachment(
        card, replace_text=None, replacement=None
    ) -> object:
        card_path = os.path.join(os.getcwd(), f"resources/{card}")
        with open(card_path, "r") as in_file:
            data = in_file.read()
            if replace_text and replacement:
                data = data.replace(replace_text, replacement)

            card_data = json.loads(data)

        return TabResponseCard(card=card_data)
