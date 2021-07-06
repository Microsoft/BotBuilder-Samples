dynamic DoStep(dynamic user, dynamic conversation, dynamic dialog, dynamic turn)
{
    var age = System.Convert.ToSingle(user["age"]);
    conversation["cat"] = age / 5;
    return age * 5;
}
