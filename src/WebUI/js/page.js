var Page = new Class({
    Implements: [ Options ],

    options: {
        container: 'page-container'
    },

    initialize: function(name, source, options) {
        this.setOptions(options);
        
        this.name = name;
        this.source = source;
    },

    // Override in child pages
    load: function() {
        new Request.HTML({
            url: this.source,
            onSuccess: function(tree, elements, html, js) {
                $("page-container").set('html', html);
                this.element = $("page-container");

                this.setup();
            }.bind(this)
        }).get();
    }
});