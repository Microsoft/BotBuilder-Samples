// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples.Models
{
    public static class UIConstants
    {
        public static UISettings YouTube { get; set; } =
            new UISettings(1000, 700, "You Tube Video", TaskModuleIds.YouTube, "You Tube");
        public static UISettings InputTextCard { get; set; } =
            new UISettings(400, 200, "Adaptive Card: Inputs", TaskModuleIds.InputTextCard, "Adaptive Card");
    }
}
