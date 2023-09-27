window.blazorBase.richTextEditor = {
    configureQuillJs: (options) => {
           
        options.modules.imageResize = {
            displaySize: true
        };
        
        return options;
    }
};