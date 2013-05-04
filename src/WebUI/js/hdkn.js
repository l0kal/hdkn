var Hadouken = new Class({
  Implements: [ Options, Events ],

  initialize: function(network) {
    this.network = network;
  },

  // Loads all plugins (eg. loads their boot.js file)
  loadPlugins: function(){

  },

  // Returns all plugins in a handy list
  listPlugins: function(callback){
    this.network.get("/api/plugins", function(data) {
      callback(data);
    });
  }
})