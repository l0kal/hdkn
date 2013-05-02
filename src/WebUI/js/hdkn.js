$(document).ready(function() {
  var fileData = [];

  jQuery.event.props.push('dataTransfer');

  $('#torrentsDropTarget').bind('drop', function(e){
    e.stopPropagation();
    e.preventDefault();
    
    var files = e.dataTransfer.files;

    $.each(files, function(index, file) {
        var fileReader = new FileReader();
        fileReader.onload = (function(f) {
          return function(ev) {
            fileData.push({
                name: f.name,
                data: this.result.split(",")[1]
            });

            console.log(this);
            $("#torrentsToAdd").append($("<li></li>").text(f.name));
          };
        })(files[index]);

        fileReader.readAsDataURL(file);
    });
  });

  $("#addtorrents_btn").bind("click", function() {
    $.each(fileData, function(index, fd) {
      var d = {
        data: fileData[index].data
      };

      $.ajax({
        type: "POST",
        contentType: "application/json",
        url: "/api/torrents",
        data: JSON.stringify(d),
        success: function() {
            console.log("success!");
        }
      })
    });
  });

  $('#torrentsDropTarget').bind('dragenter', function(e) {
    $(this).addClass("active");
    return false;
  });

  $('#torrentsDropTarget').bind('dragleave', function(e) {
    $(this).removeClass("active");
    return false;
  });
});