import React, { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import './LocationPicker.css';

// Fix for default marker icon
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});

L.Marker.prototype.options.icon = DefaultIcon;

// Component to handle map clicks
const MapClickHandler = ({ onLocationSelect }) => {
    useMapEvents({
        click(e) {
            const { lat, lng } = e.latlng;
            onLocationSelect(lat, lng);
        }
    });
    return null;
};

// Component to update map view when coordinates change
const MapViewController = ({ center }) => {
    const map = useMap();

    useEffect(() => {
        if (center) {
            map.setView(center, map.getZoom());
        }
    }, [center, map]);

    return null;
};

const LocationPicker = ({ onLocationSelect, initialLat = null, initialLng = null }) => {
    // Default to Sofia, Bulgaria if no initial coordinates
    const defaultCenter = [42.6977, 23.3219];
    const [position, setPosition] = useState(
        initialLat && initialLng ? [initialLat, initialLng] : null
    );
    const [mapCenter, setMapCenter] = useState(
        initialLat && initialLng ? [initialLat, initialLng] : defaultCenter
    );
    const [addressInfo, setAddressInfo] = useState(null);
    const [loading, setLoading] = useState(false);

    // Reverse geocoding using Nominatim API
    const reverseGeocode = async (lat, lng) => {
        setLoading(true);
        try {
            const response = await fetch(
                `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`,
                {
                    headers: {
                        'Accept-Language': 'bg,en'
                    }
                }
            );

            if (!response.ok) {
                throw new Error('Geocoding failed');
            }

            const data = await response.json();
            const address = data.address || {};

            // Extract relevant address components
            const city = address.city || address.town || address.village || address.municipality || '';
            const country = address.country || '';
            const street = address.road || address.street || '';
            const houseNumber = address.house_number || '';
            const suburb = address.suburb || address.neighbourhood || '';

            // Build full address
            let fullAddress = '';
            if (street) {
                fullAddress = street;
                if (houseNumber) {
                    fullAddress += ' ' + houseNumber;
                }
            }
            if (suburb && !fullAddress.includes(suburb)) {
                fullAddress = fullAddress ? `${fullAddress}, ${suburb}` : suburb;
            }

            const info = {
                city,
                country,
                address: fullAddress || data.display_name?.split(',')[0] || ''
            };

            setAddressInfo(info);
            return info;
        } catch (error) {
            console.error('Reverse geocoding error:', error);
            return null;
        } finally {
            setLoading(false);
        }
    };

    const handleMapClick = async (lat, lng) => {
        const newPosition = [lat, lng];
        setPosition(newPosition);

        // Fetch address info and pass everything to parent
        const info = await reverseGeocode(lat, lng);
        onLocationSelect({
            lat,
            lng,
            city: info?.city || '',
            country: info?.country || '',
            address: info?.address || ''
        });
    };

    const handleMarkerDrag = async (e) => {
        const { lat, lng } = e.target.getLatLng();
        setPosition([lat, lng]);

        // Fetch address info and pass everything to parent
        const info = await reverseGeocode(lat, lng);
        onLocationSelect({
            lat,
            lng,
            city: info?.city || '',
            country: info?.country || '',
            address: info?.address || ''
        });
    };

    // Update position when initial coordinates change
    useEffect(() => {
        if (initialLat && initialLng) {
            const newPosition = [initialLat, initialLng];
            setPosition(newPosition);
            setMapCenter(newPosition);
        }
    }, [initialLat, initialLng]);

    return (
        <div className="location-picker">
            <div className="location-picker-instructions">
                <p>📍 Кликнете на картата за избор на местоположението на хотела</p>
                {loading && (
                    <div className="geocoding-loading">
                        ⏳ Зареждане на адрес...
                    </div>
                )}
                {position && !loading && (
                    <div className="coordinates-display">
                        <span className="coordinate">
                            <strong>Lat:</strong> {position[0].toFixed(6)}
                        </span>
                        <span className="coordinate">
                            <strong>Lng:</strong> {position[1].toFixed(6)}
                        </span>
                    </div>
                )}
                {addressInfo && !loading && (
                    <div className="address-display">
                        {addressInfo.address && (
                            <span className="address-item">
                                <strong>📍 Адрес:</strong> {addressInfo.address}
                            </span>
                        )}
                        {addressInfo.city && (
                            <span className="address-item">
                                <strong>🏙️ Град:</strong> {addressInfo.city}
                            </span>
                        )}
                        {addressInfo.country && (
                            <span className="address-item">
                                <strong>🌍 Държава:</strong> {addressInfo.country}
                            </span>
                        )}
                    </div>
                )}
            </div>

            <MapContainer
                center={mapCenter}
                zoom={13}
                className="location-picker-map"
                scrollWheelZoom={true}
            >
                <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />

                <MapClickHandler onLocationSelect={handleMapClick} />
                <MapViewController center={mapCenter} />

                {position && (
                    <Marker
                        position={position}
                        draggable={true}
                        eventHandlers={{
                            dragend: handleMarkerDrag
                        }}
                    />
                )}
            </MapContainer>

            {!position && (
                <div className="location-hint">
                    Изберете местоположение на картата или въведете координати ръчно
                </div>
            )}
        </div>
    );
};

export default LocationPicker;
