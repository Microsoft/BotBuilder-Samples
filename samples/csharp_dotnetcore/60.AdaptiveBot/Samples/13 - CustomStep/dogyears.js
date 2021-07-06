function doStep(user, conversation, dialog, turn) {
    if (user.age)
        return user.age * 7;
    return 0;
}