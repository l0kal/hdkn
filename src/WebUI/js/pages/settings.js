var SettingType = {
    "Boolean": 0,
    "Integer": 1,
    "String": 2
};

var Settings = new Class({
    Extends: Page,

    initialize: function(hdkn) {
        // get an instance of 'hdkn'
        this.hdkn = hdkn;

        this.parent("Settings", "/pages/settings.html");
    },

    setup: function() {
        this.hdkn.listPlugins(function(plugins) {
            this.listPlugins(plugins);
        }.bind(this));

        this.hdkn.listSettings(function(settings) {
            this.listSettings(settings);
        }.bind(this));

        var that = this;

        this.element.getElements(".setlink").addEvent('click', function(e){
            e.preventDefault();
            e.stopPropagation();

            that.element.getElements(".setlink").getParent().removeClass("active");
            this.getParent().addClass("active");

            that.element.getElements(".settings-pane").addClass("hide");
            that.element.getElementById("pane-" + this.get("href").substring(1)).removeClass("hide");
        });

        this.element.getElementById("settings_save_btn").addEvent('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            var elem = this;

            elem.set('disabled', true);

            var inputs = that.element.getElements("input");
            var settings = {};

            for(var i = 0; i < inputs.length; i++) {
                var input = inputs[i];
                var name = input.get('name');

                switch(input.get('type')) {
                    case "checkbox":
                        settings[name] = input.checked;
                        break;

                    default:
                        settings[name] = input.get('value');
                }

                if(isNumeric(settings[name])) {
                    settings[name] = parseInt(settings[name], 10);
                }
            }

            that.hdkn.network.post("/api/settings", settings, function(response) {
                elem.set('disabled', false);
            });
        });
    },

    listPlugins: function(plugins) {
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
    },

    listSettings: function(settings) {
        for(var i = 0; i < settings.length; i++){
            var setting = settings[i];
            var element = $$('input[name=' + setting.key + ']');

            if(element != null) {
                switch(setting.type) {
                    case SettingType.String:
                        element.set('value', setting.value);
                        break;

                    case SettingType.Boolean:
                        element.set('checked', setting.value);
                        break;
                }
            }
        }
    }
});