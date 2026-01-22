window.initInvoiceMap = (invoices, dotNetRef, apiKey) => {
    if (!window.google) {
        const script = document.createElement('script');
        script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}`;
        script.onload = () => window.initInvoiceMap(invoices, dotNetRef, apiKey);
        document.head.appendChild(script);
        return;
    }
    const map = new google.maps.Map(document.getElementById('gmap'), {
        center: { lat: invoices[0]?.latitude || 18.5, lng: invoices[0]?.longitude || -69.9 },
        zoom: 9,
        streetViewControl: true
    });
    window.invoiceMarkers = {};
    invoices.forEach(inv => {
        const color = inv.geocodingStatus === 'Success' ? 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
            : inv.geocodingStatus === 'Ambiguous' ? 'http://maps.google.com/mapfiles/ms/icons/yellow-dot.png'
            : 'http://maps.google.com/mapfiles/ms/icons/red-dot.png';
        const marker = new google.maps.Marker({
            position: { lat: inv.latitude, lng: inv.longitude },
            map,
            draggable: true,
            icon: color,
            title: inv.invoiceNumber
        });
        marker.addListener('dragend', e => {
            dotNetRef.invokeMethodAsync('UpdateInvoiceLocation', inv.invoiceNumber, e.latLng.lat(), e.latLng.lng());
        });
        marker.addListener('click', () => {
            map.panTo(marker.getPosition());
            new google.maps.InfoWindow({ content: `<b>${inv.invoiceNumber}</b><br>${inv.clientName}` }).open(map, marker);
        });
        window.invoiceMarkers[inv.invoiceNumber] = marker;
    });
    map.addListener('rightclick', e => {
        if (window.selectedInvoice) {
            const marker = window.invoiceMarkers[window.selectedInvoice];
            marker.setPosition(e.latLng);
            const inv = invoices.find(x => x.invoiceNumber === window.selectedInvoice);
            inv.latitude = e.latLng.lat();
            inv.longitude = e.latLng.lng();
            dotNetRef.invokeMethodAsync('UpdateInvoiceLocation', window.selectedInvoice, e.latLng.lat(), e.latLng.lng());
        }
    });
    window.selectInvoiceOnMap = invoiceNumber => {
        window.selectedInvoice = invoiceNumber;
        const marker = window.invoiceMarkers[invoiceNumber];
        if (marker) {
            map.panTo(marker.getPosition());
            new google.maps.InfoWindow({ content: `<b>${invoiceNumber}</b>` }).open(map, marker);
        }
    };
};
