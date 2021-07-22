// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

class UISettings {
    constructor(width, height, title, id, buttonTitle) {
        this.width = width;
        this.height = height;
        this.title = title;
        this.id = id;
        this.buttonTitle = buttonTitle;
    }
}

module.exports.UISettings = UISettings;
