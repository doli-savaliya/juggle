var ko = require("knockout");
var $ = require("jquery");
require("jquery-ui/datepicker");


ko.bindingHandlers.datePicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		var options = ko.utils.unwrapObservable(valueAccessor()) || {};

		var valueBinding = allBindingsAccessor.get("value");

		$(element).datepicker(Object.assign({}, options, {
			onSelect: function() {
				if (valueBinding) {
					valueBinding(element.value);
				}
			}
		}));
	}
};
