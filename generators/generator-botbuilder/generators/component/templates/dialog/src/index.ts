// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { BotComponent } from "botbuilder-core";
import { ComponentDeclarativeTypes } from "botbuilder-dialogs-declarative";
import { MultiplyDialog } from "./multiplyDialog";

import {
  Configuration,
  ServiceCollection,
} from "botbuilder-dialogs-adaptive-runtime-core";

export default class MyBotComponent extends BotComponent {
  configureServices(
    services: ServiceCollection,
    configuration: Configuration
  ): void {
    services.composeFactory<ComponentDeclarativeTypes[]>(
      "declarativeTypes",
      (declarativeTypes) => [
        ...declarativeTypes,
        {
          getDeclarativeTypes() {
            return [
              {
                kind: MultiplyDialog.$kind,
                type: MultiplyDialog,
              },
            ];
          },
        },
      ]
    );
  }
}
