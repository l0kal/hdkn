var Dialog = new Class({
    Implements: [ Options, Events ],

    options: {
        loaded: null
    },

    initialize: function(source, options) {
        this.setOptions(options);

        var r = new Request.HTML({
            url: source
        });

        r.addEvent('success', function(tree, elements, html, js) {
            var container = new Element('div', {
                id: 'thedialog',
                html: html
            });

            container.getElements(".close").addEvent('click', function() { this.hide(); }.bind(this));

            container.setStyle('display', 'none');
            container.inject(document.body);

            this.element = container;
            this.load();

            this.options.loaded();
        }.bind(this));

        r.get();
    },

    // override in child class to hook up events
    load: function(){},

    show: function(modal) {
        if(modal) $('modal-back').setStyle('display', 'block');
        this.element.setStyle('display', 'block');
    },

    showDialog: function(){
        this.show(true);
    },

    hide: function() {
        this.element.dispose();
        $('modal-back').setStyle('display', 'none');
    }
});