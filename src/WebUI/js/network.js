// Network related stuffs

var Network = new Class({
    initialize: function(){

    },

    post: function(url, data, callback) {
        var r = new Request({
            data: JSON.encode(data),
            url: url,
            urlEncoded: false,
            onSuccess: function(json) {
                callback(json);
            }
        });

        r.setHeader("Content-Type", "application/json");
        r.post();
    }
});