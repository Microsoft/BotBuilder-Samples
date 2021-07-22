// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

class SkillsConfiguration {
    constructor() {
        this.skillsData = {};

        // Note: we only have one skill in this sample but we could load more if needed.
        const botFrameworkSkill = {
            id: process.env.SkillId,
            appId: process.env.SkillAppId,
            skillEndpoint: process.env.SkillEndpoint
        };

        this.skillsData[botFrameworkSkill.id] = botFrameworkSkill;

        this.skillHostEndpointValue = process.env.SkillHostEndpoint;
        if (!this.skillHostEndpointValue) {
            throw new Error('[SkillsConfiguration]: Missing configuration parameter. SkillHostEndpoint is required');
        }
    }

    get skills() {
        return this.skillsData;
    }

    get skillHostEndpoint() {
        return this.skillHostEndpointValue;
    }
}

module.exports.SkillsConfiguration = SkillsConfiguration;
