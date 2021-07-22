/* eslint-disable */
module.exports = {
    "extends": "standard",
    "rules": {
        "semi": [2, "always"],
        "indent": [2, 4, { "SwitchCase": 1 }],
        "no-return-await": 0,
        "space-before-function-paren": [2, {
            "named": "never",
            "anonymous": "never",
            "asyncArrow": "always"
        }],
        "template-curly-spacing": [2, "always"]
    },
    "env": {
        "commonjs": true,
        "node": true,
        "mocha": true
    }
};