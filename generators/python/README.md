# python-generator-botbuilder

Cookiecutter generators for [Bot Framework v4](https://dev.botframework.com).  Will let you quickly set up a conversational AI bot
using core AI capabilities.

## About

`python-generator-botbuilder` will help you build new conversational AI bots using the [Bot Framework v4](https://dev.botframework.com).

## Templates

The generator supports three different template options.  The table below can help guide which template is right for you.

|  Template  |  Description  |
| ---------- |  ---------  |
| Echo&nbsp;Bot | A good template if you want a little more than "Hello World!", but not much more.  This template handles the very basics of sending messages to a bot, and having the bot process the messages by repeating them back to the user.  This template produces a bot that simply "echoes" back to the user anything the user says to the bot. |
| Core&nbsp;Bot | Our most advanced template, the Core template provides 6 core features every bot is likely to have.  This template covers the core features of a Conversational-AI bot using [LUIS](https://www.luis.ai).  See the **Core Bot Features** table below for more details. |
| Empty&nbsp;Bot | A good template if you are familiar with Bot Framework v4, and simply want a basic skeleton project.  Also a good option if you want to take sample code from the documentation and paste it into a minimal bot in order to learn. |

### How to Choose a Template

| Template | When This Template is a Good Choice |
| -------- | -------- |
| Echo&nbsp;Bot  | You are new to Bot Framework v4 and want a working bot with minimal features. |
| Core&nbsp;Bot | You understand some of the core concepts of Bot Framework v4 and are beyond the concepts introduced in the Echo Bot template.  You're familiar with or are ready to learn concepts such as language understanding using LUIS, managing multi-turn conversations with Dialogs, handling user initiated Dialog interruptions, and using Adaptive Cards to welcome your users. |
| Empty&nbsp;Bot  | You are a seasoned Bot Framework v4 developer.  You've built bots before, and want the minimum skeleton of a bot. |

### Template Overview

#### Echo Bot Template

The Echo Bot template is slightly more than the a classic "Hello World!" example, but not by much.  This template shows the basic structure of a bot, how a bot recieves messages from a user, and how a bot sends messages to a user.  The bot will "echo" back to the user, what the user says to the bot.  It is a good choice for first time, new to Bot Framework v4 developers.

#### Core Bot Template

The Core Bot template consists of set of core features most every bot is likely to have.  Building off of the core message processing features found in the Echo Bot template, this template adds a number of more sophisticated features.  The table below lists these features and provides links to additional documentation.

| Core&nbsp;Bot&nbsp;Features | Description |
| ------------------ | ----------- |
| [Send and receive messages](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-send-messages?view=azure-bot-service-4.0) | The primary way your bot will communicate with users, and likewise receive communication, is through message activities. Some messages may simply consist of plain text, while others may contain richer content such as cards or attachments. |
| [Proactive messaging](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0) using [Adaptive Cards](https://docs.microsoft.com/azure/bot-service/bot-builder-send-welcome-message?view=azure-bot-service-4.0?#using-adaptive-card-greeting) | The primary goal when creating any bot is to engage your user in a meaningful conversation. One of the best ways to achieve this goal is to ensure that from the moment a user first connects to your bot, they understand your bot’s main purpose and capabilities.  We refer to this as "welcoming the user."  The Core template uses an [Adaptive Card](http://adaptivecards.io) to implement this behavior.  |
| [Language understanding using LUIS](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-luis?view=azure-bot-service-4.0) | The ability to understand what your user means conversationally and contextually can be a difficult task, but can provide your bot a more natural conversation feel. Language Understanding, called LUIS, enables you to do just that so that your bot can recognize the intent of user messages, allow for more natural language from your user, and better direct the conversation flow. |
| [Multi-turn conversation support using Dialogs](https://docs.microsoft.com/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0) | The ability to manage conversations is an important part of the bot/user interation.  Bot Framework introduces the  concept of a Dialog to handle this conversational pattern.  Dialog objects process inbound Activities and generate outbound responses. The business logic of the bot runs either directly or indirectly within Dialog classes.  |
| [Managing conversation state](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state?view=azure-bot-service-4.0) | A key to good bot design is to track the context of a conversation, so that your bot remembers things like the answers to previous questions. |
| [How to handle user-initiated interruptions](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-handle-user-interrupt?view=azure-bot-service-4.0) | While you may think that your users will follow your defined conversation flow step by step, chances are good that they will change their minds or ask a question in the middle of the process instead of answering the question. Handling interruptions means making sure your bot is prepared to handle situations like this. |
| [How to unit test a bot](https://aka.ms/cs-unit-test-docs) | Optionally, the Core Bot template can generate corresponding unit tests that shows how to use the testing framework introduced in Bot Framework version 4.5.  Selecting this option provides a complete set of units tests for Core Bot.  It shows how to write unit tests to test the various features of Core Bot. To add the Core Bot unit tests, run the generator and answer `yes` when prompted.  See below for an example of how to do this from the command line.  |

#### Empty Bot Template

The Empty Bot template is the minimal skeleton code for a bot.  It provides a stub `on_turn` handler but does not perform any actions.  If you are experienced writing bots with Bot Framework v4 and want the minimum scaffolding, the Empty template is for you.

## Features by Template

|  Feature  |  Empty&nbsp;Bot  |  Echo&nbsp;Bot   |  Core&nbsp;Bot*  |
| --------- | :-----: | :-----: | :-----: |
| Generate code in Python | X | X | X |
| Support local development and testing using the [Bot Framework Emulator v4](https://www.github.com/microsoft/botframework-emulator) | X | X | X |
| Core bot message processing |  | X | X |
| Deploy your bot to Microsoft Azure |  | Pending | Pending |
| Welcome new users using Adaptive Card technology |  |  | X |
| Support AI-based greetings using [LUIS](https://www.luis.ai) |  |  | X |
| Use Dialogs to manage more in-depth conversations |  |  | X |
| Manage conversation state |  |  | X |
| Handle user interruptions |  |  | X |
| Unit test a bot using Bot Framework Testing framework (optional) |  |  | X |

*Core Bot template is a work in progress landing soon. 
## Installation

1. Install [cookiecutter](https://github.com/cookiecutter/cookiecutter) using [pip](https://pip.pypa.io/en/stable/) (we assume you have pre-installed [python 3](https://www.python.org/downloads/)).

    ```bash
    pip install cookiecutter
    ```

2. Verify that cookiecutter has been installed correctly by typing the following into your console:

    ```bash
    cookiecutter --help
    ```


## Usage

### Creating a New Bot Project

To create an Echo Bot project:

```bash
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/echo.zip
```

To create a Core Bot project:

```bash
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/core.zip
```

To create an Empty Bot project:

```bash
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/empty.zip
```

When the generator is launched, it will prompt for the information required to create a new bot.

### Generator Command Line Options and Arguments

Cookiecutter supports a set of pre-defined command line options, the complete list with descriptions is available [here](https://cookiecutter.readthedocs.io/en/0.9.1/advanced_usage.html#command-line-options).

Each generator can recieve a series of named arguments to pre-seed the prompt default value. If the `--no-input` option flag is send, these named arguments will be the default values for the template.

| Named&nbsp;argument  | Description |
| ------------------- | ----------- |
| project_name    | The name given to the bot project |
| bot_description | A brief bit of text that describes the purpose of the bot |
| add_tests        | **PENDING** _A Core Bot Template Only Feature_.  The generator will add unit tests to the Core Bot generated bot.  This option is not available to other templates at this time.  To learn more about the test framework released with Bot Framework v4.5, see [How to unit test bots](https://aka.ms/js-unit-test-docs).  This option is intended to enable automated bot generation for testing purposes. |

#### Example Using Named Arguments

This example shows how to pass named arguments to the generator, setting the default bot name to test_project.

```bash
# Run the generator defaulting the bot name to test_project
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/echo.zip project_name="test_project"
```

### Generating a Bot Using --no-input

The generator can be run in `--no-input` mode, which can be used for automated bot creation.  When run in `--no-input` mode, the generator can be configured using named arguments as documented above.  If a named argument is ommitted a reasonable default will be used.

#### Default Values

| Named&nbsp;argument  | Default Value |
| ------------------- | ----------- |
| bot_name     | `my-chat-bot` |
| bot_description | "Demonstrate the core capabilities of the Microsoft Bot Framework" |
| add_tests        | `False`|

#### Examples Using --no-input

This example shows how to run the generator in --no-input mode, setting all required options on the command line.

```bash
# Run the generator, setting all command line options
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/echo.zip --no-input project_name="test_bot" bot_description="Test description"
```

This example shows how to run the generator in --no-input mode, using all the default command line options.  The generator will create a bot project using all the default values specified in the **Default Options** table above.

```bash
# Run the generator using all default options
cookiecutter https://github.com/microsoft/botbuilder-samples/releases/download/Templates/echo.zip --no-input
```

This example shows how to run the generator in --no-input mode, with unit tests.

```bash
# PENDING: Run the generator using all default options
```

## Running Your Bot

### Running Your Bot Locally

To run your bot locally, type the following in your console:

```bash
# install dependencies
pip install -r requirements.txt
```

```bash
# run the bot
python app.py
```

Alternatively to the last command, you can set the file in an environment variable with `set FLASK_APP=app.py` in windows (`export FLASK_APP=app.py` in mac/linux) and then run `flask run --host=127.0.0.1 --port=3978`

### Interacting With Your Bot Using the Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

Once the Emulator is connected, you can interact with and receive messages from your bot.

#### Lint Compliant Code

The code generated by the botbuilder generator is pylint compliant to our ruleset. To use pylint as your develop your bot:

```bash
# Assuming you created a project with the bot_name value 'my_chat_bot'
pylint --rcfile=my_chat_bot/.pylintrc my_chat_bot
```

#### Testing Core Bots with Tests (Pending)

Core Bot templates generated with unit tests can be tested using the following:

```bash
# launch pytest
pytest
```

## Deploy Your Bot to Azure

After creating the bot and testing it locally, you can deploy it to Azure to make it accessible from anywhere.
To learn how, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete set of deployment instructions.

If you are new to Microsoft Azure, please refer to [Getting started with Azure](https://azure.microsoft.com/get-started/) for guidance on how to get started on Azure.

## Logging Issues and Providing Feedback

Issues and feedback about the botbuilder generator can be submitted through the project's [GitHub Issues](https://github.com/Microsoft/botbuilder-samples/issues) page.
