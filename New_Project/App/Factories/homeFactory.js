app.service('homeFactory', function ($http) {
    var vm = this;

    vm.getall = function () {
        return $http.get('api/Users');
    }

    vm.getId = function (id) {
        return $http.get('api/Users/'+ id);
    }

    vm.deleteIndex = function () {
        return $http.delete('api/Users');
    }

    vm.refresh = function () {
        return $http.get('api/Users/arr/test')
    }
});