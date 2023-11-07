//The function initializes the map from the Geocoder API to show on our interface for the pickup location site

function initMap() {

    const map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: -17.824858, lng: 31.053028 },
        zoom: 14,
    });
    const input = document.getElementById("pac-input");
    // Specify just the place data fields that you need.
    const autocomplete = new google.maps.places.Autocomplete(input, {
        fields: ["place_id", "geometry", "name", "formatted_address"],
    });

    autocomplete.bindTo("bounds", map);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    const infowindow = new google.maps.InfoWindow();
    const infowindowContent = document.getElementById("infowindow-content");

    infowindow.setContent(infowindowContent);

    const geocoder = new google.maps.Geocoder();
    const marker = new google.maps.Marker({ map: map });

    marker.addListener("click", () => {
        infowindow.open(map, marker);
    });
    autocomplete.addListener("place_changed", () => {
        infowindow.close();

        const place = autocomplete.getPlace();

        if (!place.place_id) {
            return;
        }

        geocoder
            .geocode({ placeId: place.place_id })
            .then(({ results }) => {
                map.setZoom(11);
                map.setCenter(results[0].geometry.location);
                // Set the position of the marker using the place ID and location.
                // @ts-ignore TODO This should be in @typings/googlemaps.
                marker.setPlace({
                    placeId: place.place_id,
                    location: results[0].geometry.location,
                });
                marker.setVisible(true);
                infowindowContent.children["place-name"].textContent = place.name;
                //infowindowContent.children["place-id"].textContent = place.place_id;
                infowindowContent.children["place-address"].textContent =
                    results[0].formatted_address;
                infowindow.open(map, marker);
            })
            .catch((e) => window.alert("Geocoder failed due to: " + e));
    });
}

initMap();
initMapDestination();



//The function initializes the map from the Geocoder API to show on our interface for the dropoff location site

function initMapDestination() {


    const map = new google.maps.Map(document.getElementById("destinationmap"), {
        center: { lat: -17.824858, lng: 31.053028 },
        zoom: 14,
    });
    const input = document.getElementById("destination-input");
    // Specify just the place data fields that you need.
    const autocomplete = new google.maps.places.Autocomplete(input, {
        fields: ["place_id", "geometry", "name", "formatted_address"],
    });

    autocomplete.bindTo("bounds", map);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    const infowindow = new google.maps.InfoWindow();
    const infowindowContent = document.getElementById("infowindow-content");

    infowindow.setContent(infowindowContent);

    const geocoder = new google.maps.Geocoder();
    const marker = new google.maps.Marker({ map: map });

    marker.addListener("click", () => {
        infowindow.open(map, marker);
    });
    autocomplete.addListener("place_changed", () => {
        infowindow.close();

        const place = autocomplete.getPlace();

        if (!place.place_id) {
            return;
        }

        geocoder
            .geocode({ placeId: place.place_id })
            .then(({ results }) => {
                map.setZoom(11);
                map.setCenter(results[0].geometry.location);
                // Set the position of the marker using the place ID and location.
                // @ts-ignore TODO This should be in @typings/googlemaps.
                marker.setPlace({
                    placeId: place.place_id,
                    location: results[0].geometry.location,
                });
                marker.setVisible(true);
                infowindowContent.children["place-name"].textContent = place.name;
                //infowindowContent.children["place-id"].textContent = place.place_id;
                infowindowContent.children["place-address"].textContent =
                    results[0].formatted_address;
                infowindow.open(map, marker);
            })
            .catch((e) => window.alert("Geocoder failed due to: " + e));
    });
}

