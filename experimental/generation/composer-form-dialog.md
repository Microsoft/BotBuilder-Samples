# Form dialogs in Bot Framework Composer

The form dialogs feature in Composer uses [dialog generation][generation] to automatically generate a dialog from a form. The generated dialog collects information for fields of the form while handling ambiguity, corrections, help, navigation and multiple fields at once. With form dialogs, Composer uses your form and creates the adaptive dialog, the bot responses, and the language model for you. The generated dialog is a child dialog that is called by your root dialog. 

**Note:** Form dialogs is a preview feature of Composer and should *not* be used in a production environment. To try an early version of form dialogs, follow the steps below.

## Enabling the form dialogs preview

To enable the form dialogs preview in Composer, follow these steps:

1. Install [Bot Framework Composer](https://docs.microsoft.com/en-us/composer/install-composer#build-composer-from-source).
2. In Composer, go to your **Settings** by clicking on the gear icon at the bottom of the menu.
4. In the **Preview features** section, select the **Form dialogs** checkbox. You should see a new icon appear in the menu. This is the **Forms** section. Once enabled, the form dialog preview will remain enabled until de-select this checkbox.
5. **Create a new bot** or **open an existing bot** from the **Home** page. The Forms section will not be enabled if you do not have a bot open.



## Creating and using a form dialog

The following steps describe how to create a form dialog, how to connect it to your root dialog, and how to re-generate your dialog to reflect changes to your form fields.


1. **Create a form**
The property schema defines the fields your bot needs to fill and values that are acceptable. Values can be defined by an enumerated list or by a type, such as age, email, geography, or number.

    1. In the menu, select the **Forms** icon. You will see this page:    
    2. Under **Schemas**, select the plus button to add a new schema. 
    3. In the **"Create a form dialog"** window, provide a name. This will be the name of your schema as well as the dialog generated from it.
    4. Under **Schemas**, select the schema you just created. The schema editor should appear on the right. 
    5. Select **Add property**.
    6. A new property card will appear. Provide a **name** and a **type**.
    7. **Drag and drop cards to change priority or to make a property optional.** By default, properties are set as `required` and prioritized by they order in the form. ***Required properties*** are properties that your bot will ask the user to provide. The ***priority*** refers to the order in which the bot will ask for the required properties. ***Optional properties*** are properties the bot accepts if given but does not ask for.

3. **Generate your dialog.** When your schema is complete, press the "Generate dialog" button. 
4. **Inspect your new form dialog**. You can navigate to your dialog by selecting the overflow menu (...) next to the name of the form. You can also use select the **Design** iconin the main menu to see your list of dialogs to view the form dialog. The form dialog will appear in this list. The triggers for the dialog will be grouped by property along with a group for form-wide triggers.

5. **Connect your form dialog to the root dialog.**
You connect your form dialog to your main dialog as you would any child dialog. For example, if you'd like to invoke your form dialog from the root, you can add a new trigger:
    1. From your root dialog, select the **Add** dropdown and select the **Add a new trigger**.
    2. In the **"Create a trigger"** window, select the trigger type **"Intent recognized"**.
   
       If your root dialog is using the default recognizer, add example trigger phrases. Add a condition `=turn.recognized.score > 0.8` to prevent your form from restarting.
    3. In your new trigger, add the action node **Manage properties > Begin a new dialog** and select your form dialog.

Alternatively, you can add a **Begin a new dialog** node as an action in an existing trigger.

You can now run your bot with your form dialog.

## Editing language assets
Dialog generation creates the .dialog files as well as the bot responses (LG) and language understanding model (LU). We recommend editing the text of the bot responses or the user inputs as opposed creating new templates or deleting existing ones.

### Editing bot responses (LG)
To edit all of the bot responses possible with your form dialog, go to the **Bot responses** section and select your form dialog. The form dialog will group all the bot responses by property. The template name indicates the context in which the response will be given, so you can edit all the bot responses for a property in one location.

[ screenshot ]
  
Alternatively, you can also edit bot responses from the flow diagram of your form dialog.

### Editing the language understanding model (LU)
Similar to bot responses, you can edit all the language understanding specific to a property in one location. Go to the "User input" section and select your form dialog. The form dialog will group all the example utterances by property.

[ Eventually -- add link to separate document explaing in the LUIS model. ]

## Updating  your form dialog
As you developer your bot you might decide to edit, add, or remove properties from your schema. You can make changes to your form and regenerate your dialog by pressing the **Generate dialog** button. Your dialog, bot responses, and language understanding model will be updating without losing your prior customizations. 
