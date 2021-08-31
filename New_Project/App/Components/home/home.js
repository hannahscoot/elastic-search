app.controller("homeCtrl", function (homeFactory) {
    var vm = this;
    vm.input = '';
    vm.responseData;
    vm.nameinput = '';
    
    vm.search = function () {//Super Simple Search
        if (vm.input == '') {
            homeFactory.getall().then(function (resp) {
                vm.responseData = resp.data;
            })
        } else {
            homeFactory.getId(vm.input).then(function (resp) {
                vm.responseData = resp.data;
            })
        }
    }

    vm.upsert = function () {
        homeFactory.upsert(vm.input, vm.nameinput).then(function (resp) {
            vm.responseData = resp.data;
        })
    }

    vm.clearAll = function () {
        homeFactory.deleteIndex().then(function (resp) {
            vm.responseData = resp.data;
        })
    }

    vm.refresh = function () {
        homeFactory.refresh().then(function (resp) {
            vm.responseData = resp.data;
        })
    }
});
