# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from .ui_settings import UISettings
from .task_module_ids import TaskModuleIds


class UIConstants:
    YOUTUBE = UISettings(1000, 700, "YouTube Video", TaskModuleIds.YOUTUBE, "YouTube")
    ADAPTIVE_CARD = UISettings(
        400,
        200,
        "Adaptive Card: Inputs",
        TaskModuleIds.ADAPTIVE_CARD,
        "Adaptive Card",
    )
