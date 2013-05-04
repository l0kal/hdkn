var Hadouken = new Class({
  Implements: [ Options, Events ],

  initialize: function(network) {
    this.network = network;
  },

  getPlugins: function(callback){
    this.network.get("/api/plugins", function(data) {
      callback(data);
    });
  }
})