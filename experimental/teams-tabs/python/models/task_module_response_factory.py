# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from typing import Union

from botbuilder.schema.teams import (
    TaskModuleResponse,
    TaskModuleMessageResponse,
    TaskModuleTaskInfo,
    TaskModuleContinueResponse,
)


class TaskModuleResponseFactory:
    @staticmethod
    def create_response(value: Union[str, TaskModuleTaskInfo]) -> TaskModuleResponse:
        if isinstance(value, TaskModuleTaskInfo):
            return TaskModuleResponse(task=TaskModuleContinueResponse(value=value))
        return TaskModuleResponse(task=TaskModuleMessageResponse(value=value))

    @staticmethod
    def to_task_module_response(task_info: TaskModuleTaskInfo):
        return TaskModuleResponseFactory.create_response(task_info)
