define(["exports", "fable-core"], function (exports, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.parseDate = parseDate;
    exports.toDate = toDate;

    function parseDate(exp) {
        var groups = _fableCore.RegExp.match(exp, "([0-9]+)-([0-9]+)\\-([0-9]+)");

        var year = Number.parseInt(groups[1]);
        var month = Number.parseInt(groups[2]);
        var day = Number.parseInt(groups[3]);
        return _fableCore.Date.create(year, month, day);
    }

    function toDate(date) {
        return _fableCore.String.fsFormat("%i-%02i-%02i")(function (x) {
            return x;
        })(_fableCore.Date.year(date))(_fableCore.Date.month(date))(_fableCore.Date.day(date));
    }
});
//# sourceMappingURL=Tools.js.map