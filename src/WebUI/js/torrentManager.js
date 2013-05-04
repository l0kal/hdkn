var TorrentManager = new Class({
    Implements: [ Options, Events ],

    options: {
        pollInterval: 3000
    },

    initialize: function(options) {
        this.setOptions(options);

        this.torrents = {};

        this.requestor = new Request({
            method: "get",
            initialDelay: 10,
            delay: this.options.pollInterval,
            url: "/api/torrents",
            onSuccess: function(data) {
                this.loadData(data);
            }.bind(this)
        });

        this.requestor.startTimer();
    },

    loadData: function(data) {

    }
});