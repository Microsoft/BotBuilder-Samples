// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { UISettings } = require('./UISettings');
const { TaskModuleIds } = require('./taskModuleIds');

const UIConstants = {
    YouTube: new UISettings(1000, 700, 'YouTube Video', TaskModuleIds.YouTube, 'YouTube'),
    InputTextCard: new UISettings(400, 200, 'Adaptive Card: Inputs', TaskModuleIds.InputTextCard, 'Adaptive Card')
};

module.exports.UIConstants = UIConstants;
