// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { JwtTokenValidation, SkillValidation } = require('botframework-connector');
const { SkillsConfiguration } = require('../skillsConfiguration');

const skillsConfig = new SkillsConfiguration();
// Load the appIds for the configured skills (we will only allow responses from skills we have configured).
const allowedSkills = Object.values(skillsConfig.skills).map(skill => skill.appId);

/**
 * Sample claims validator that loads an allowed list from configuration if present
 * and checks that responses are coming from configured skills.
 */
const allowedSkillsClaimsValidator = async (claims) => {
    // For security, developer must specify allowedSkills.
    if (!allowedSkills || allowedSkills.length === 0) {
        throw new Error('AllowedCallers not specified in .env.');
    }
    if (!allowedSkills.includes('*') && SkillValidation.isSkillClaim(claims)) {
        // Check that the appId claim in the skill request is in the list of skills configured for this bot.
        const appId = JwtTokenValidation.getAppIdFromClaims(claims);
        if (!allowedSkills.includes(appId)) {
            throw new Error(`Received a request from an application with an appID of "${ appId }". To enable requests from this skill, add the skill to your configuration file.`);
        }
    }
};

module.exports.allowedSkillsClaimsValidator = allowedSkillsClaimsValidator;
