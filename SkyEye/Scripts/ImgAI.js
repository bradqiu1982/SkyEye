var ImgAI = function () {

    var demo = function () {
        function analyzeimg()
        {
            var fpath = $('#imgfolder').val();
            if (fpath == '')
            {
                alert('Image Folder should not be empty!');
                return false;
            }

            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/VCSELAIDemoData', {
                fpath: fpath
            }, function (output) {

                $.bootstrapLoading.end();

                $('.imgcontent').empty();
                $.each(output.urllist, function (i, val) {

                    var appendstr = '<div class="row form-group"><div class="col-xs-2"></div><div class="col-xs-8">';
                    appendstr += '<img src="'+val+'" />'
                    appendstr += '</div><div class="col-xs-2"></div></div>'
                    appendstr += '<hr>';

                    $(".imgcontent").append(appendstr);
                });

            });

        }

        $('body').on('click', '#btn-option', function () {
            analyzeimg();
        })
    }
    
    return {
        DemoFun: function () {
            demo();
        }
    }

}();