window.onload = function () {

    let timeOptions = { hour: '2-digit', minute: '2-digit' };
    let dateOptions = { year: 'numeric', month: '2-digit', day: '2-digit' };
    var locations = [];
    var vehicles = [];
    var drivers = [];
    var requests = [];
    var requestsCount = 0;
    var transports = [];
    var layers = [];

    var map = L.map('map').setView([52, 21], 8);
    var layersControl = L.control.layers(null, null, { 'collapsed': false });
    layersControl.addTo(map);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        notdelete: true
    }).addTo(map);


    fetch('index.json')
        .then(response => response.json())
        .then(data => displayProblems(data, 'problem-selector-container'));

    function displayData(data) {
        populateCollections(data);
        displayDescription(data, 'problem-container');
        displayOnMap(data);
        displaySummary(data, 'problem-id', 'problem-summary');
    }

    function populateCollections(data) {
        locations = [];
        requests = [];
        vehicles = [];
        drivers = [];
        transports = [];
        requestsCount = data.Requests.length;

        for (var i = 0; i < data.Requests.length; ++i) {
            locations[data.Requests[i].DeliveryLocation.Id] = data.Requests[i].DeliveryLocation;
            locations[data.Requests[i].PickupLocation.Id] = data.Requests[i].PickupLocation;
            requests[data.Requests[i].Id] = data.Requests[i];
        }

        if (data.Drivers && data.Drivers.length > 0) {
            data.Drivers.sort(function (a, b) {
                if (a.Id > b.Id) {
                    return 1;
                } else if (b.Id > a.Id) {
                    return -1;
                } else {
                    return 0;
                }
            });
    
    
            for (var i = 0; i < data.Drivers.length; ++i) {
                if (data.Drivers[i].Id) {
                    drivers[data.Drivers[i].Id] = data.Drivers[i];
                } else {
                    drivers[i] = data.Drivers[i];
                }
            }    
        }

        data.Vehicles.sort(function (a, b) {
            if (a.Id > b.Id) {
                return 1;
            } else if (b.Id > a.Id) {
                return -1;
            } else {
                return 0;
            }
        });


        for (var i = 0; i < data.Vehicles.length; ++i) {
            if (data.Vehicles[i].Id) {
                vehicles[data.Vehicles[i].Id] = data.Vehicles[i];
            } else {
                vehicles[i] = data.Vehicles[i];
            }
        }

        if (data.Solutions && data.Solutions.length > 0) {
            transports = data.Solutions[data.Solutions.length - 1].Transports;
            transports.sort(function (a, b) {
                if (a.TractorId > b.TractorId) {
                    return 1;
                } else if (b.TractorId > a.TractorId) {
                    return -1;
                } else {
                    if (a.TrailerTruckId > b.TrailerTruckId) {
                        return 1;
                    } else if (b.TrailerTruckId > a.TrailerTruckId) {
                        return -1;
                    } else {
                        if (a.AvailableForLoadingTime > b.AvailableForLoadingTime) {
                            return 1;
                        } else if (b.AvailableForLoadingTime > a.AvailableForLoadingTime) {
                            return -1;
                        } else {
                            return 0;
                        }
                    }
                }
            });
        }

    }

    function displayProblems(data, containerId) {
        var div = document.getElementById(containerId);
        var select = document.createElement('select');
        for (var i = 0; i < data.problems.length; ++i) {
            var option = document.createElement('option');
            option.value = data.problems[i].file;
            option.innerHTML = data.problems[i].label;
            select.appendChild(option);
        }
        select.onchange = function () {
            fetch(this.value)
                .then(response => response.json())
                .then(data => displayData(data));
        }
        div.appendChild(select);
        fetch(select.value)
            .then(response => response.json())
            .then(data => displayData(data));
    }

    function displaySummary(data, titleId, contentId) {
        document.getElementById(titleId).innerHTML = data.Client + ' ' +
            (new Date(data.Date)).toLocaleDateString("pl-PL", dateOptions) + ' ' + data.DepotId;
        document.getElementById(contentId).innerHTML = '';
        if (data.Solutions) {
            for (var i = 0; i < data.Solutions.length; ++i) {
                var totalDelay = 0;
                var maxDelay = 0;
                var maxEarlyArrival = 0;
                var countDelays = 0;
                var travelTime = 0;
                var serviceTime = 0;
                var meanFillInRatio = 0.0;
                var earlyArrival = 0.0;
                var countUnknownRequests = 0;
                for (var tidx = 0; tidx < data.Solutions[i].Transports.length; ++tidx) {
                    meanFillInRatio += data.Solutions[i].Transports[tidx].FillInRatio;
                    for (var sidx = 0; sidx < data.Solutions[i].Transports[tidx].Schedule.length; ++sidx) {
                        var currentDelay = data.Solutions[i].Transports[tidx].Schedule[sidx].Delay
                        totalDelay += currentDelay;
                        maxDelay = Math.max(maxDelay, currentDelay);
                        if (currentDelay > 0) {
                            countDelays++;
                        }
                        if (data.Solutions[i].Transports[tidx].Schedule[sidx].LocationId != data.DepotId) {
                            serviceTime += data.Solutions[i].Transports[tidx].Schedule[sidx].DepartureTime - data.Solutions[i].Transports[tidx].Schedule[sidx].ArrivalTime;
                        }
                        if (sidx > 0) {
                            travelTime += data.Solutions[i].Transports[tidx].Schedule[sidx].ArrivalTime - data.Solutions[i].Transports[tidx].Schedule[sidx - 1].DepartureTime;
                        }
                        var currentEarlyArrival = 0;
                        for (var uidx = 0; uidx < data.Solutions[i].Transports[tidx].Schedule[sidx].UnloadedRequestsIds.length; ++uidx) {
                            var unloadedId = data.Solutions[i].Transports[tidx].Schedule[sidx].UnloadedRequestsIds[uidx];
                            try {
                                currentEarlyArrival = Math.max(currentEarlyArrival, requests[unloadedId].DeliveryPreferedTimeWindowStart -
                                    data.Solutions[i].Transports[tidx].Schedule[sidx].ArrivalTime);
                                console.info(requests[unloadedId]);
                                console.info(requests[unloadedId].DeliveryPreferedTimeWindowStart);
                                console.info(data.Solutions[i].Transports[tidx].Schedule[sidx].ArrivalTime);
                            } catch (err) {
                                console.error('Unknown request ' + unloadedId);
                                countUnknownRequests++;
                            }
                        }
                        earlyArrival += currentEarlyArrival;
                        maxEarlyArrival = Math.max(maxEarlyArrival, currentEarlyArrival);
                    }
                }
                meanFillInRatio /= data.Solutions[i].Transports.length;
                var uniqueVehicleIds = data.Solutions[i].Transports
                    .map(t => t.TrailerTruckId)
                    .filter((v, i, a) => a.indexOf(v) === i);
                var uniqueTractorIds = data.Solutions[i].Transports
                    .map(t => t.TractorId)
                    .filter((v, i, a) => a.indexOf(v) === i);
                console.info(data.Solutions[i].Transports[0]);
                document.getElementById(contentId).innerHTML +=
                    '<table>' +
                    '<tr><th style="text-align: left">Algorytm</th><td style="text-align: right">' + data.Solutions[i].Algorithm +
                    ':' + data.Solutions[i].Version + '</td></tr>' +
                    '<tr><th style="text-align: left">Długość tras</th><td style="text-align: right">' + Number(data.Solutions[i].TotalLength).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Czas postoju</th><td style="text-align: right">' + Number(serviceTime).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Czas jazdy</th><td style="text-align: right">' + Number(travelTime).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Suma spóźnień</th><td style="text-align: right">' + Number(totalDelay).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Suma zbyt wczesnych przyjazdów</th><td style="text-align: right">' + Number(earlyArrival).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Maksymalne spóźnienie</th><td style="text-align: right">' + Number(maxDelay).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Maksymalny zbyt wczesny przyjazd</th><td style="text-align: right">' + Number(maxEarlyArrival).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Liczba punktów ze spóźnieniem</th><td style="text-align: right">' + Number(countDelays).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Stopień wypełnienia</th><td style="text-align: right">' + Number(meanFillInRatio).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Liczba transportów</th><td style="text-align: right">' + Number(data.Solutions[i].Transports.length).toLocaleString() + '</td></tr>' +
                    '<tr><th style="text-align: left">Liczba znanych zleceń</th><td style="text-align: right">' + requestsCount + '</td></tr>' +
                    '<tr><th style="text-align: left">Nieprzypisane zlecenia</th><td style="text-align: right">' + data.Solutions[i].LeftRequestsIds.length + '</td></tr>' +
                    '<tr><th style="text-align: left">Nadmiarowe zlecenia</th><td style="text-align: right">' + countUnknownRequests + '</td></tr>' +
                    '<tr><th style="text-align: left">Liczba wykorzystanych pojazdów z pojemnością</th><td style="text-align: right">' + uniqueVehicleIds.length + '</td></tr>' +
                    '<tr><th style="text-align: left">Liczba wykorzystanych ciągników</th><td style="text-align: right">' + uniqueTractorIds.length + '</td></tr>' +
                    '</table>';
            }
        } else {
            document.getElementById(contentId).innerHTML = "Brak wyników"
        }
    }

    function getDateFromZeroHourAndOffset(zeroHour, offset) {
        var d = new Date(zeroHour);
        return new Date(d.getTime() + offset * 1000);
    }

    function displayDescription(data, descriptionId) {
        var descriptionContainer = document.getElementById(descriptionId);
        descriptionContainer.innerHTML = '';
        generateTransportsTable(descriptionContainer);
        generateRequestsTable(descriptionContainer);
        generateVehiclesTable(descriptionContainer);
        generateDriversTable(descriptionContainer);

        function generateTransportsTable(descriptionContainer) {
            descriptionContainer.innerHTML += '<h1>Transporty</h1>';
            var transportsTable = document.createElement('table');
            var tr = document.createElement('tr');
            var td = document.createElement('th');
            td.innerHTML = 'Id pojazdu';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Id kierowcy';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Wypełnienie';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Długość trasy';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Liczba punktów';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Wielkość pojazdu';
            if (transports.length > 0) {
                td.colSpan = vehicles[transports[0].TrailerTruckId].Capacity.length;
            }
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Czas pracy';
            tr.appendChild(td);
            transportsTable.appendChild(tr);

            for (var i = 0; i < transports.length; ++i) {
                var tr = document.createElement('tr');
                var td = document.createElement('td');
                td.innerHTML = transports[i].TractorId + ':' + transports[i].TrailerTruckId;
                tr.appendChild(td);
                td = document.createElement('td');
                td.innerHTML = transports[i].DriverId;
                tr.appendChild(td);
                td = document.createElement('td');
                td.innerHTML = transports[i].FillInRatio.toLocaleString();
                tr.appendChild(td);
                td = document.createElement('td');
                td.innerHTML = transports[i].Length.toLocaleString();
                td.style.textAlign = 'right';
                tr.appendChild(td);
                td = document.createElement('td');
                td.innerHTML = transports[i].Schedule.length - 2;
                td.style.textAlign = 'right';
                tr.appendChild(td);
                try {
                    for (var c = 0; c < vehicles[transports[i].TrailerTruckId].Capacity.length; ++c) {
                        td = document.createElement('td');
                        td.style.textAlign = 'right';
                        td.innerHTML = vehicles[transports[i].TrailerTruckId].Capacity[c];
                        tr.appendChild(td);
                    }
                } catch {
                    td = document.createElement('td');
                    td.style.textAlign = 'right';
                    td.innerHTML = 'Nieznany pojazd ' + transports[i].TrailerTruckId;
                    tr.appendChild(td);
                    console.error('Unknown vehicle ' + transports[i].TrailerTruckId);
                }
                td = document.createElement('td');
                var availableForLoading = transports[i].Schedule[0].ArrivalTime;
                var finished = transports[i].Schedule[transports[i].Schedule.length - 1].DepartureTime;
                td.innerHTML = getDateFromZeroHourAndOffset(data.ZeroHour, availableForLoading)
                    .toLocaleTimeString("pl-PL", timeOptions);
                td.innerHTML += ' - ' + getDateFromZeroHourAndOffset(data.ZeroHour, finished)
                    .toLocaleTimeString("pl-PL", timeOptions);
                td.innerHTML += ' (' + (Math.round((finished - availableForLoading) / 3600 * 100) / 100) + ')';
                tr.appendChild(td);
                transportsTable.appendChild(tr);
            }
            descriptionContainer.appendChild(transportsTable);
        }

        function generateRequestsTable(descriptionContainer) {
            descriptionContainer.innerHTML += '<h1>Zlecenia transportu</h1>';
            var vehiclesTable = document.createElement('table');
            var tr = document.createElement('tr');
            var td = document.createElement('th');
            td.innerHTML = 'Id';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Rozmiar';
            for (var key in requests) {
                var request = requests[key];
                td.colSpan = request.Size.length;
                break;
            }
            tr.appendChild(td);
            var td = document.createElement('th');
            td.innerHTML = 'Lokalizacja<br />załadunku';
            tr.appendChild(td);
            var td = document.createElement('th');
            td.innerHTML = 'Lokalizacja<br />rozładunku';
            tr.appendChild(td);
            var td = document.createElement('th');
            td.innerHTML = 'Czas załadunku';
            tr.appendChild(td);
            var td = document.createElement('th');
            td.innerHTML = 'Czas rozładunku';
            tr.appendChild(td);
            vehiclesTable.appendChild(tr);


            for (var key in requests) {
                var request = requests[key];
                var tr = document.createElement('tr');
                var td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = request.Id;
                tr.appendChild(td);

                for (var c = 0; c < request.Size.length; ++c) {
                    td = document.createElement('td');
                    td.style.textAlign = 'right';
                    td.innerHTML = Math.round(request.Size[c] * 100) / 100;
                    tr.appendChild(td);
                }

                var td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = request.PickupLocation.Id;
                tr.appendChild(td);

                var td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = request.DeliveryLocation.Id;
                tr.appendChild(td);

                var td = document.createElement('td');
                td.style.textAlign = 'center';
                if (Number.isFinite(2 * request.PickupPreferedTimeWindowStart) && Number.isFinite(2 * request.PickupPreferedTimeWindowEnd))
                {
                    td.innerHTML = getDateFromZeroHourAndOffset(data.ZeroHour, request.PickupPreferedTimeWindowStart)
                    .toLocaleTimeString("pl-PL", timeOptions) + ' - ' + getDateFromZeroHourAndOffset(data.ZeroHour, request.PickupPreferedTimeWindowEnd)
                    .toLocaleTimeString("pl-PL", timeOptions);
                }
                else
                {
                    td.innerHTML = "Bez ograniczeń";
                }
                tr.appendChild(td);

                var td = document.createElement('td');
                td.style.textAlign = 'center';
                if (Number.isFinite(2 * request.DeliveryPreferedTimeWindowStart) && Number.isFinite(2 * request.DeliveryPreferedTimeWindowEnd))
                {
                    td.innerHTML = getDateFromZeroHourAndOffset(data.ZeroHour, request.DeliveryPreferedTimeWindowStart)
                    .toLocaleTimeString("pl-PL", timeOptions) + ' - ' + getDateFromZeroHourAndOffset(data.ZeroHour, request.DeliveryPreferedTimeWindowEnd)
                    .toLocaleTimeString("pl-PL", timeOptions);
                }
                else
                {
                    td.innerHTML = "Bez ograniczeń";
                }
                tr.appendChild(td);

                vehiclesTable.appendChild(tr);
            }
            descriptionContainer.appendChild(vehiclesTable);
        }

        function generateVehiclesTable(descriptionContainer) {
            descriptionContainer.innerHTML += '<h1>Pojazdy</h1>';
            var vehiclesTable = document.createElement('table');
            var tr = document.createElement('tr');
            var td = document.createElement('th');
            td.innerHTML = 'Id';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Własność';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Pojemność';
            for (var key in vehicles) {
                var vehicle = vehicles[key];
                td.colSpan = vehicle.Capacity.length;
                break;
            }
            tr.appendChild(td);

            td = document.createElement('th');
            td.innerHTML = 'Dostępność';
            tr.appendChild(td);

            vehiclesTable.appendChild(tr);


            for (var key in vehicles) {
                var vehicle = vehicles[key];
                var tr = document.createElement('tr');
                var td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = vehicle.Id;
                tr.appendChild(td);

                td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = (vehicle.OwnerID == '1' ? 'Własny ' : 'Przewoźnik ') + vehicle.OwnerID;
                tr.appendChild(td);

                for (var c = 0; c < vehicle.Capacity.length; ++c) {
                    td = document.createElement('td');
                    td.style.textAlign = 'right';
                    td.innerHTML = vehicle.Capacity[c];
                    tr.appendChild(td);
                }

                td = document.createElement('td');
                td.style.textAlign = 'center';
                td.innerHTML = getDateFromZeroHourAndOffset(data.ZeroHour, vehicle.AvailabilityStart)
                .toLocaleTimeString("pl-PL", timeOptions);
                td.innerHTML += ' - ';
                td.innerHTML += getDateFromZeroHourAndOffset(data.ZeroHour, vehicle.AvailabilityEnd)
                .toLocaleTimeString("pl-PL", timeOptions);
                tr.appendChild(td);

                vehiclesTable.appendChild(tr);
            }
            descriptionContainer.appendChild(vehiclesTable);
        }

        function generateDriversTable(descriptionContainer) {
            descriptionContainer.innerHTML += '<h1>Kierowcy</h1>';
            var driversTable = document.createElement('table');
            var tr = document.createElement('tr');
            var td = document.createElement('th');
            td.innerHTML = 'Id';
            tr.appendChild(td);
            td = document.createElement('th');
            td.innerHTML = 'Dostępność';
            tr.appendChild(td);
            driversTable.appendChild(tr);


            for (var key in drivers) {
                var driver = drivers[key];
                var tr = document.createElement('tr');
                var td = document.createElement('td');
                td.style.textAlign = 'right';
                td.innerHTML = driver.Id;
                tr.appendChild(td);

                td = document.createElement('td');
                td.style.textAlign = 'center';
                td.innerHTML = getDateFromZeroHourAndOffset(data.ZeroHour, driver.AvailabilityStart)
                .toLocaleTimeString("pl-PL", timeOptions);
                td.innerHTML += ' - ';
                td.innerHTML += getDateFromZeroHourAndOffset(data.ZeroHour, driver.AvailabilityEnd)
                .toLocaleTimeString("pl-PL", timeOptions);
                tr.appendChild(td);

                driversTable.appendChild(tr);
            }
            descriptionContainer.appendChild(driversTable);
        }
    }

    function displayOnMap(data) {
        for (var i = 0; i < layers.length; ++i) {
            map.removeLayer(layers[i]);
            layersControl.removeLayer(layers[i]);
        }
        layers = [];
        var markerArray = [];
        for (var i = 0; i < data.Requests.length; ++i) {
            if (data.DepotId == data.Requests[i].PickupLocation.Id) {
                var markerDelivery = createRequestMarker(data.ZeroHour, data.Requests[i], data.Requests[i].DeliveryLocation, 0);
                markerArray.push(markerDelivery);
            }

            if (data.DepotId == data.Requests[i].DeliveryLocation.Id) {
                var markerPickup = createRequestMarker(data.ZeroHour, data.Requests[i], data.Requests[i].PickupLocation, 1);
                markerArray.push(markerPickup);
            }
        }
        var group = L
            .featureGroup(markerArray)
            .addTo(map);
        layersControl.addOverlay(group, 'Zlecenia');
        layers.push(group);
        map.fitBounds(group.getBounds());

        for (var i = 0; i < transports.length; ++i) {
            var schedule = transports[i].Schedule;
            var points = [];
            for (var j = 0; j < schedule.length; ++j) {
                try {
                    points.push([locations[schedule[j].LocationId].Lat, locations[schedule[j].LocationId].Lng]);
                } catch {
                    console.error('Unknown location ' + schedule[j].LocationId);
                }
            }
            var polyline = L.polyline(points);
            polyline.addTo(map);
            layersControl.addOverlay(polyline, transports[i].TractorId + ':' + transports[i].TrailerTruckId);
            layers.push(polyline);
        }
    }

    function createRequestMarker(zeroHour, request, location, requestType) {
        /*
        '<table><tr><th>Zlecenie</th><th>Lokalizacja</th></tr>' +
                        '<tr><td>' + request.Id + '</td><td>' + location.Id + '</td></tr></table>'
                        */
        var table = document.createElement('table');
        table.style.textAlign = 'center';
        var tr = document.createElement('tr');
        var th = document.createElement('th');
        th.innerHTML = 'Zlecenie'
        tr.appendChild(th);
        th = document.createElement('th');
        th.innerHTML = 'Lok';
        tr.appendChild(th);
        th = document.createElement('th');
        th.innerHTML = 'Typ'
        tr.appendChild(th);
        th = document.createElement('th');
        th.innerHTML = 'Rozmiar'
        th.colSpan = request.Size.length;
        tr.appendChild(th);
        th = document.createElement('th');
        th.innerHTML = 'Maks. pojazd'
        tr.appendChild(th);
        table.appendChild(tr);
        th = document.createElement('th');
        th.innerHTML = 'Okno czas.'
        tr.appendChild(th);
        table.appendChild(tr);

        var tr = document.createElement('tr');
        var td = document.createElement('td');
        td.innerHTML = request.Id;
        tr.appendChild(td);

        td = document.createElement('td');
        td.innerHTML = location.Id;
        tr.appendChild(td);

        td = document.createElement('td');
        td.innerHTML = requestType;
        tr.appendChild(td);

        for (var s = 0; s < request.Size.length; ++s) {
            td = document.createElement('td');
            td.innerHTML = request.Size[s];
            tr.appendChild(td);
        }
        td = document.createElement('td');
        td.innerHTML = request.MaxVehicleSize.EpCount;
        tr.appendChild(td);

        td = document.createElement('td');
        td.innerHTML = getDateFromZeroHourAndOffset(
            zeroHour,
            request.DeliveryPreferedTimeWindowStart).toLocaleTimeString("pl-PL", timeOptions);
        td.innerHTML += ' ' + getDateFromZeroHourAndOffset(
            zeroHour,
            request.DeliveryPreferedTimeWindowEnd).toLocaleTimeString("pl-PL", timeOptions);
        tr.appendChild(td);


        table.appendChild(tr);

        return L
            .marker([location.Lat, location.Lng])
            .bindPopup(table, { closeOnClick: false, autoClose: false });

    }
}