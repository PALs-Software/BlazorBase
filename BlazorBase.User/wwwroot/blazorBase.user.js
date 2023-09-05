window.blazorBase.user = {
    submitForm: function (id) {
        document.getElementById(id).submit();
    },
    submitLoginFormOnEnter: function (args, id) {
        if (args.key !== 'Enter')
            return;
        args.target.blur();
        document.getElementById(id).click();
        args.target.focus();
    }
};