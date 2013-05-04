var Settings = new Class({
    Extends: Page,

    initialize: function(hdkn) {
        // get an instance of 'hdkn'
        this.hdkn = hdkn;

        this.parent("Settings", "/pages/settings.html");
    },

    setup: function() {
        this.hdkn.getPlugins(function(plugins) {
            this.loadPlugins(plugins);
        }.bind(this));
    },

    loadPlugins: function(plugins) {
        var pluginHeader = this.element.getElementById("settings_plugins");

        for(var i = 0; i < plugins.length; i++){
            var plugin = plugins[i];

            var li = new Element('li');
            var a = new Element('a', {
                href: "#",
                html: plugin.name + "-" + plugin.version,
                events: {
                    click: function(e) {
                        e.preventDefault();
                        e.stopPropagation();

                        console.log(plugin.name);
                    }
                }
            });

            a.inject(li);
            li.inject(pluginHeader, 'after');
        }
    }
});