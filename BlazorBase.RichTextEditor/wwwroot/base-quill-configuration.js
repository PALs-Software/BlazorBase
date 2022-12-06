window.blazorBase = {
    version: "1.0.0.0"
};

window.blazorBase.richTextEditor = {
    configureQuillJs: (options) => {
           
        options.modules.imageResize = {
            displaySize: true
        };
        
        return options;
    }
};