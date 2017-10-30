(function () {

    angular.module(APP.NAME).controller('ImagePicker', ImagePicker);

    ImagePicker.$inject = [ "$log",  "cmsService",  "Domain", "$uibModalInstance", "toastr", "fileService", "model"];

    function ImagePicker( $log, cmsService, Domain, $uibModalInstance, toastr, fileService, model) {

        var vm = this;
        vm.domain = Domain;
        vm.close = _close;
        vm.$onInit = _onInit;
        vm.newValue = _newValue;


        function _close() {
            $uibModalInstance.dismiss();
        }

        function _onInit() {
            fileService.getUploadedFiles(_getFilesSuccess, _getFilesError);
           
        }

        function _getFilesSuccess(response) {

            $log.log(response);

            vm.images = response.data.items;
        }

        function _getFilesError(response) {
            toastr.error(response.data);
        }

        function _newValue(image) {

            if (model.id > 0) {

                model.value = image.fileName;
               
                cmsService.changeContent(model, __valuesSuccess, _updateValuesError);
                
            }
            else {

                model.value = image.fileName;

                var object = {
                    "templateKeyId": model.keyId,
                    "Value": image.fileName,
                    "CMSPageId": model.pageId
                };
                cmsService.addNewContent(object, __valuesSuccess, _newContentError);
                
            }
        }
        //---------------------------------------------------  success and error  -------------------------------------------------------//

        function __valuesSuccess(response) {
            toastr.success("Success...");
            $uibModalInstance.close();
            
        }

        function _updateValuesError(response) {
            toastr.error(response.data, "Error...");
        }

       

        function _newContentError(response) {
            toastr.error(response.data);
        }

    }
})();