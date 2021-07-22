// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const Generator = require("yeoman-generator");
const chalk = require("chalk");
const t = require("runtypes");

const pkg = require("../../package.json");
const version = pkg.version;

const templates = ["empty", "dialog"];

module.exports = class extends Generator {
    constructor(args, opts) {
        super(args, opts);

        this.option("name", {
            desc: "The name you want to give to your bot component",
            type: String,
            default: "my-bot-component",
            alias: "N",
        });

        this.option("description", {
            desc: "A brief bit of text used to describe what your bot does",
            type: String,
            default:
                "Demonstrate the core capabilities of the Microsoft Bot Framework Adaptive Runtime",
            alias: "D",
        });

        this.option("template", {
            desc: `Initial bot component capabilities. ("${templates.join(
                '" | "'
            )}")`,
            type: String,
            default: templates[0],
            alias: "T",
        });

        this.option("noprompt", {
            desc: "Do not prompt for any information or confirmation",
            type: Boolean,
            required: false,
            default: false,
            alias: "Y",
        });
    }

    initializing() {
        this.log(
            `\nWelcome to the Microsoft Bot Builder generator v${version}. `
        );
    }

    async prompting() {
        if (this.options.noprompt) {
            return;
        }

        Object.assign(
            this.options,
            await this.prompt([
                {
                    type: "input",
                    name: "name",
                    message: "What's the name of your bot component?",
                    default: this.options.name,
                },
                {
                    type: "input",
                    name: "description",
                    message: "What will your bot component do?",
                    default: this.options.description,
                },
                {
                    type: "list",
                    name: "template",
                    message: "Which template would you like to start with?",
                    choices: [
                        {
                            name: "empty",
                            value: "empty",
                        },
                        {
                            name: "dialog",
                            value: "dialog",
                        },
                    ],
                    default: this.options.template,
                },
                {
                    type: "confirm",
                    name: "confirmed",
                    message:
                        "Looking good.  Shall I go ahead and create your new bot?",
                    default: true,
                },
            ])
        );
    }

    writing() {
        const nonEmptyString = t.String.withConstraint(
            (str) => str.length > 0 || "must be non-empty"
        );

        try {
            const { name, description, template } = t
                .Record({
                    name: nonEmptyString,
                    description: nonEmptyString,
                    template: nonEmptyString.withConstraint(
                        (template) =>
                            templates.indexOf(template) !== -1 ||
                            `must be one of "${templates.join('" | "')}"`
                    ),
                })
                .check(this.options);

            // Change destination root so installation stage operates inside bot directory
            this.destinationRoot(name);

            this.fs.copyTpl(
                this.templatePath(template),
                this.destinationPath(),
                {
                    name,
                    description,
                    version,
                },
                {},
                {
                    globOptions: {
                        dot: true,
                    },
                }
            );
        } catch (err) {
            if (err instanceof t.ValidationError) {
                this.log(chalk.red("\nERROR: Unable to validate options:"));
                Object.entries(err.details).forEach(([key, value]) => {
                    this.log(`- ${key}: ${value}`);
                });
            } else {
                throw err;
            }
        }
    }

    install() {
        if (this.options.confirmed) {
            this.installDependencies({ bower: false });
        }
    }

    end() {
        if (this.options.confirmed) {
            this.log(chalk.green("---------------------------------- "));
            this.log(chalk.green(" Your new bot component is ready!  "));
            this.log(chalk.green("---------------------------------- "));
        } else {
            this.log(
                chalk.red.bold("----------------------------------------- ")
            );
            this.log(
                chalk.red.bold(" New bot component creation was canceled. ")
            );
            this.log(
                chalk.red.bold("----------------------------------------- ")
            );
        }

        this.log("Thank you for using the Microsoft Bot Framework. ");
        this.log("\n< ** > The Bot Framework Team");
    }
};
