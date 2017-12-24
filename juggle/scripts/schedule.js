import ko from "knockout";
import $ from "jquery";
import moment from "moment";
import _ from "underscore";
import touchScrollify from "touchscroller";
import {translate3d} from "touchscroller";
import {} from "custom-bindings";

{
  window.ko = ko;

  const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

  // assume 8 hr day with 1 hr timeblocks
  // eventually will be supplied from app config.
  const createDayInfo = (dateMoment, dayStart, dayEnd, timeblockLength) => {
    dateMoment = (dateMoment) ? dateMoment : moment().startOf('day');

    let start = dateMoment
      .clone()
      .startOf("day")
      .add(dayStart, "hours");
    let end = dateMoment
      .clone()
      .startOf("day")
      .add(dayEnd, "hours");

    let length = end.hour() - start.hour();
    let numTimeblocks = length / timeblockLength;

    return {
      date: dateMoment,
      start,
      end,
      length,
      numTimeblocks,
      timeblockLength,
    };
  };

  const createEventVm = (ev) => {
    let timeRangeStart = moment(ev.appointment.timeRangeStart);
    let timeRangeEnd = moment(ev.appointment.timeRangeEnd);

    let _vm = {
      id: ev.id,
      recurring: ev.appointment.recurring,
      employeeId: ev.employeeId,
      clientId: ev.appointment.clientId,
      date: moment(ev.date),
      length: moment.duration(ev.appointment.length, "minutes"),
      appointmentId: ev.appointment.Id,
      defaultEmployeeId: ev.appointment.defaultEmployeeId,
      description: ev.appointment.description,
      day: ev.appointment.day,
      workTypes: ev.appointment.workTypes,
      startDate: moment(ev.appointment.startDate),
      endDate: moment(ev.appointment.endDate),
      timeRangeStart: moment(ev.date)
        .add(timeRangeStart.hours(), "hours")
        .add(timeRangeStart.minutes(), "minutes"),
      timeRangeEnd: moment(ev.date)
        .add(timeRangeEnd.hours(), "hours")
        .add(timeRangeEnd.minutes(), "minutes"),
      notes: ev.appointment.notes,
    };

    return _vm;
  };

  const createAbstractRowVm = (() => {

    const createTimeblockVm = (order, dayInfo) => {
      let {timeblockLength, start, end} = dayInfo;

      let _tb = {
        start: start
          .clone()
          .add(timeblockLength * order, "hours"),
        end: start
          .clone()
          .add((timeblockLength * order) + timeblockLength, "hours"),
        length: moment.duration(timeblockLength, "hours"),
        available: true,
        order: order,
        events: ko.observableArray(),
        get sumEventsLength () {
          return _tb.events()
          .reduce((tally, e) => tally + e.length.asMinutes(), 0);
        },
        isWithinEventWindow: (ev) =>
          _tb.start.isSameOrAfter(ev.timeRangeStart)
          && _tb.end.isSameOrBefore(ev.timeRangeEnd),
        hasTimeForEvent: (ev) =>
          ev.length.asMinutes() + _tb.sumEventsLength < _tb.length.asMinutes(),
      };

      return _tb;
    };

    const createAbstractRowVm = (dayInfo, eventAllocation) => {
      let {numTimeblocks} = dayInfo;

      let timeblocks = Array(numTimeblocks)
      .fill(1)
      .map((n, i) => createTimeblockVm(i, dayInfo));

      let _vm = {
        timeblocks: timeblocks,
        addEvents: (events) => {
          let unallocatedEvents = [];
          for (let e of events) {
            let eligibleTimeblock =
              _vm.timeblocks.find((tb) => eventAllocation(tb, e));

            // move from events to timeblock.events:
            if (eligibleTimeblock) eligibleTimeblock.events.push(e);
            else unallocatedEvents.push(e);
          }

          return  unallocatedEvents;
        },
      };

      return _vm;
    };

    return createAbstractRowVm;
  })();

  const createHopperVm = (() => {
    const hopperEventAllocation = (tb, e) => tb.isWithinEventWindow(e);

    const createHopperVm =
      (dayInfo) => createAbstractRowVm(dayInfo, hopperEventAllocation);

    return createHopperVm;
  })();

  const createEmployeeVm = (employee, dayInfo) => {
    const employeeEventAllocation = (tb, e) => {
      let isWithinEventWindow = tb.isWithinEventWindow(e);
      let hasTimeForEvent = tb.hasTimeForEvent(e);
      return isWithinEventWindow && hasTimeForEvent;
    };

    let _vm = createAbstractRowVm(dayInfo, employeeEventAllocation);

    _vm.transportation = employee.transportation;
    _vm.travelTime = employee.travelTime;
    _vm.contactInfo = employee.contactInfo;
    _vm.attributes = employee.attributes;
    _vm.name = employee.name;
    _vm.id = employee.id;

    return _vm;
  };

  const createRibbonVm = (scheduleVm) => {

    let _ribbon = {
      expanded: ko.observable(false),
      selectedEvent: ko.observable(null),
      toggleExpand: () => _ribbon.expanded(!_ribbon.expanded()),
    };

    const getRibbonSelectedEmployee = () => {
      let selectedEvent = _ribbon.selectedEvent();
      let hasSelectedEvent = selectedEvent !== null;
      let hasAssignedEmployee = (hasSelectedEvent)
        ? _ribbon.selectedEvent().employeeId !== EMPTY_GUID
        : false;

      if (hasSelectedEvent && hasAssignedEmployee)
        return scheduleVm.employees.find((e) => e.id === selectedEvent.employeeId);

      return null;
    };
    _ribbon.selectedEmployee = ko.computed(getRibbonSelectedEmployee);

    const getRibbonSelectedClient = () => {
      let selectedEvent = _ribbon.selectedEvent();
      if (selectedEvent !== null)
        return scheduleVm.clients.find((c) => c.id === selectedEvent.clientId);

      return null;
    };
    _ribbon.selectedClient = ko.computed(getRibbonSelectedClient);

    return _ribbon;
  };

  const createBowlsVm = (schedule) => {

    const dayInfo = createDayInfo(moment(schedule.date), 9, 17, 1);

    // initialize hopper:
    const hopper = (() => {
      let hopper = createHopperVm(dayInfo);
      let hopperEvents = schedule
        .hopperEvents
        .map((ev) => createEventVm(ev, dayInfo));
      hopper.addEvents(hopperEvents);

      return hopper;
    })();

    // initialize employees:
    const employees = schedule.employees.map((e) => {
      let _vm = createEmployeeVm(e, dayInfo);
      let employeeEvents = e.events.map((ev) => createEventVm(ev, dayInfo));
      let leftoverEvents = _vm.addEvents(employeeEvents);
      hopper.addEvents(leftoverEvents);

      return _vm;
    });

    const _vm = {
      dayInfo: dayInfo,
      hopper: hopper,
      employees: employees,
      clients: schedule.clients,
    };

    _vm.ribbon = createRibbonVm(_vm);

    return _vm;
  };

  const createScheduleVm = (dateMoment) => {

    const _vm = {
      date: ko.observable(dateMoment || moment().startOf("day")),
      showDatepicker: ko.observable(false),
      scheduleLoaded: ko.observable(false),
      bowls: ko.observable(null),
    };

    const initTouchScroller = () => {
      let empSched = document.querySelector(".employee-schedules");
      let hopperRow = document.querySelector(".hopper .hopper-row");
      let empInfo = document.querySelector(".schedule .keys");

      let scheduleContainerRect = document
        .querySelector(".schedule-container")
        .getBoundingClientRect();

      let empSchedRect = empSched.getBoundingClientRect();

      touchScrollify(empSched, {
        dragLimits: {
          x: -(empSchedRect.width - (scheduleContainerRect.width - 152)),
          y: -(empSchedRect.height - (scheduleContainerRect.height - 202)),
        },
      });
      touchScrollify(hopperRow, {scrollY: false});
      touchScrollify(empInfo, {scrollX: false});

      empSched.addEventListener("touchscrollermove", (e) => {
        hopperRow.style.transform = translate3d(e.detail.translateX, 0, 0);
        empInfo.style.transform = translate3d(0, e.detail.translateY, 0);
      });

      hopperRow.addEventListener("touchscrollermove", (e) => {
        empSched.style.transform = translate3d(e.detail.translateX, 0, 0);
      });

      empInfo.addEventListener("touchscrollermove", (e) => {
        empSched.style.transform = translate3d(empSched.dataset.translateX, e.detail.translateY, 0);
      });

    };

    const getSchedule = (dateMoment) => {
      _vm.bowls(null);
      _vm.scheduleLoaded(false);

      return $.ajax({
        url: `/api/schedule/?date=${ko.unwrap(dateMoment).format('YYYY-MM-DD')}`,
        method: "GET",
      })
      .done((schedule) => {
        _vm.bowls(createBowlsVm(schedule));
        _vm.scheduleLoaded(true);
        initTouchScroller();
      });
    };

    _vm.date.subscribe((newVal) => getSchedule(newVal));

    return _vm;
  };

  const scheduleVm = createScheduleVm();
  scheduleVm.date(moment().startOf("day"));

  ko.applyBindings(scheduleVm);
}
