var Page = new Class({
    initialize: function(name, source) {
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