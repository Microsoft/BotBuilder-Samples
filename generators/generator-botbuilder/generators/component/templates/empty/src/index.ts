// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { BotComponent } from "botbuilder-core";

import {
  Configuration,
  ServiceCollection,
} from "botbuilder-dialogs-adaptive-runtime-core";

export default class MyBotComponent extends BotComponent {
  configureServices(
    services: ServiceCollection,
    configuration: Configuration
  ): void {}
}
