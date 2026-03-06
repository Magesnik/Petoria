import React, { useState, useEffect } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';
import './AvailabilityCalendar.css';

const AvailabilityCalendar = ({ hotelId, roomTypes = [], isModeratorMode = false, onDateSelect }) => {
    const { t, language } = useLanguage();
    const safeRoomTypes = Array.isArray(roomTypes) ? roomTypes : [];
    const [currentMonth, setCurrentMonth] = useState(new Date());
    const [availability, setAvailability] = useState([]);
    const [selectedRoomType, setSelectedRoomType] = useState(null);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    // Use local date string to avoid UTC timezone shifting (e.g. UTC+2 shifts dates back 1 day)
    const toLocalDateStr = (date) => {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    };

    // Selection state for bulk edit
    const [selectionStart, setSelectionStart] = useState(null);
    const [selectionEnd, setSelectionEnd] = useState(null);
    const [showBulkEdit, setShowBulkEdit] = useState(false);
    const [bulkEditCount, setBulkEditCount] = useState(0);
    const [discountPercentage, setDiscountPercentage] = useState(0);

    useEffect(() => {
        if (safeRoomTypes.length > 0 && !selectedRoomType) {
            setSelectedRoomType(safeRoomTypes[0]);
        }
    }, [safeRoomTypes, selectedRoomType]);

    useEffect(() => {
        if (selectedRoomType) {
            fetchAvailability();
        }
    }, [selectedRoomType, currentMonth]);

    const fetchAvailability = async () => {
        try {
            const startOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1);
            const endOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 2, 0);

            const fromStr = toLocalDateStr(startOfMonth);
            const toStr = toLocalDateStr(endOfMonth);

            const data = await api.get(
                `/hotels/${hotelId}/availability?from=${fromStr}&to=${toStr}`
            );

            setAvailability(data);
        } catch (err) {
            console.error('Error fetching availability:', err);
        }
    };

    const getAvailabilityForDate = (date) => {
        if (!selectedRoomType) return null;
        const dateStr = toLocalDateStr(date);
        return availability.find(
            a => a.roomTypeId === selectedRoomType.id && a.date.split('T')[0] === dateStr
        );
    };

    const handleDayClick = (date) => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (date < today) return; // Don't allow selecting past dates

        if (!selectionStart) {
            setSelectionStart(date);
            setSelectionEnd(date);
        } else if (!selectionEnd || selectionStart.getTime() === selectionEnd.getTime()) {
            let start = selectionStart;
            let end = date;

            if (date < selectionStart) {
                setSelectionEnd(selectionStart);
                setSelectionStart(date);
                start = date;
                end = selectionStart;
            } else {
                setSelectionEnd(date);
            }

            if (isModeratorMode && onDateSelect) {
                onDateSelect({ start, end }, selectedRoomType);
                // Reset selection after a short delay or let parent handle it
                // For now, let's keep it selected visually
            } else {
                setShowBulkEdit(true);
                // Get current count and discount for selected date range
                const avail = getAvailabilityForDate(start);
                setBulkEditCount(avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0);
                setDiscountPercentage(avail?.discountPercentage ?? 0);
            }
        } else {
            // Reset selection
            setSelectionStart(date);
            setSelectionEnd(date);
            setShowBulkEdit(false);
        }
    };

    const isDateInSelection = (date) => {
        if (!selectionStart || !selectionEnd) return false;
        return date >= selectionStart && date <= selectionEnd;
    };

    const handleBulkUpdate = async () => {
        if (!selectionStart || !selectionEnd || !selectedRoomType) return;

        setError('');
        setSuccess('');

        try {
            // Update availability
            await api.post(
                `/hotels/${hotelId}/availability/bulk`,
                {
                    roomTypeId: selectedRoomType.id,
                    startDate: selectionStart.toISOString(),
                    endDate: selectionEnd.toISOString(),
                    availableCount: bulkEditCount
                }
            );

            // If discount percentage is set, create/update discount
            if (discountPercentage > 0) {
                await api.post(
                    `/hotels/${hotelId}/discounts`,
                    {
                        roomTypeId: selectedRoomType.id,
                        startDate: selectionStart.toISOString(),
                        endDate: selectionEnd.toISOString(),
                        discountPercentage: discountPercentage
                    }
                );
            }

            setSuccess(discountPercentage > 0
                ? t('availabilityAndDiscountUpdated')
                : t('availabilityUpdated'));
            setShowBulkEdit(false);
            setSelectionStart(null);
            setSelectionEnd(null);
            setDiscountPercentage(0);
            fetchAvailability();
        } catch (err) {
            setError(err.message || t('errorUpdating'));
        }
    };

    const handleBlockDates = async (block) => {
        if (!selectionStart || !selectionEnd) return;

        setError('');
        setSuccess('');

        try {
            await api.put(
                `/hotels/${hotelId}/availability/block`,
                {
                    roomTypeId: selectedRoomType?.id,
                    startDate: selectionStart.toISOString(),
                    endDate: selectionEnd.toISOString(),
                    isBlocked: block
                }
            );

            setSuccess(block ? t('datesBlocked') : t('datesUnblocked'));
            setShowBulkEdit(false);
            setSelectionStart(null);
            setSelectionEnd(null);
            fetchAvailability();
        } catch {
            setError(t('errorBlocking'));
        }
    };

    const previousMonth = () => {
        setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1, 1));
    };

    const nextMonth = () => {
        setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 1));
    };

    const getDaysInMonth = () => {
        const year = currentMonth.getFullYear();
        const month = currentMonth.getMonth();
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const days = [];

        // Add empty days for padding
        for (let i = 0; i < firstDay.getDay(); i++) {
            days.push(null);
        }

        // Add days of month
        for (let i = 1; i <= lastDay.getDate(); i++) {
            days.push(new Date(year, month, i));
        }

        return days;
    };

    const formatMonth = (date) => {
        const months = [
            t('monthJan'), t('monthFeb'), t('monthMar'), t('monthApr'),
            t('monthMay'), t('monthJun'), t('monthJul'), t('monthAug'),
            t('monthSep'), t('monthOct'), t('monthNov'), t('monthDec')
        ];
        return `${months[date.getMonth()]} ${date.getFullYear()}`;
    };

    if (safeRoomTypes.length === 0) {
        return (
            <div className="availability-calendar empty">
                <h3>📅 {t('availabilityCalendar')}</h3>
                <p className="empty-message">
                    {t('addRoomTypesFirst')}
                </p>
            </div>
        );
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return (
        <div className="availability-calendar">
            <h3>📅 {t('availabilityCalendar')}</h3>

            {error && <div className="calendar-error">{error}</div>}
            {success && <div className="calendar-success">{success}</div>}

            {/* Room Type Selector */}
            <div className="room-type-selector">
                <label>{t('roomType')}:</label>
                <div className="room-type-buttons">
                    {safeRoomTypes.map(room => (
                        <button
                            key={room.id}
                            className={`room-type-btn ${selectedRoomType?.id === room.id ? 'active' : ''}`}
                            onClick={() => {
                                setSelectedRoomType(room);
                                setSelectionStart(null);
                                setSelectionEnd(null);
                                setShowBulkEdit(false);
                            }}
                        >
                            {room.name}
                            <span className="room-count">({room.totalRooms} {t('rooms')})</span>
                        </button>
                    ))}
                </div>
            </div>

            {/* Calendar Navigation */}
            <div className="calendar-nav">
                <button onClick={previousMonth}>‹</button>
                <span className="current-month">{formatMonth(currentMonth)}</span>
                <button onClick={nextMonth}>›</button>
            </div>

            {/* Calendar Grid */}
            <div className="calendar-grid">
                <div className="day-header">{t('weekdaySun')}</div>
                <div className="day-header">{t('weekdayMon')}</div>
                <div className="day-header">{t('weekdayTue')}</div>
                <div className="day-header">{t('weekdayWed')}</div>
                <div className="day-header">{t('weekdayThu')}</div>
                <div className="day-header">{t('weekdayFri')}</div>
                <div className="day-header">{t('weekdaySat')}</div>

                {getDaysInMonth().map((date, index) => {
                    if (!date) {
                        return <div key={index} className="day-cell empty"></div>;
                    }

                    const avail = getAvailabilityForDate(date);
                    const isPast = date < today;
                    const isSelected = isDateInSelection(date);
                    const isBlocked = avail?.isBlocked;
                    const availableCount = avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0;
                    const isFull = availableCount === 0;
                    const hasDiscount = avail?.discountPercentage > 0;

                    return (
                        <div
                            key={index}
                            className={`day-cell 
                                ${isPast ? 'past' : ''} 
                                ${isSelected ? 'selected' : ''} 
                                ${isBlocked ? 'blocked' : ''}
                                ${isFull && !isBlocked ? 'full' : ''}
                                ${availableCount > 0 && !isBlocked ? 'available' : ''}
                                ${hasDiscount && availableCount > 0 && !isBlocked ? 'discounted' : ''}
                            `}
                            onClick={() => !isPast && handleDayClick(date)}
                        >
                            {hasDiscount && !isPast && !isBlocked && (
                                <span className="calendar-discount-badge">-{avail.discountPercentage}%</span>
                            )}
                            <span className="day-number">{date.getDate()}</span>
                            {!isPast && selectedRoomType && (
                                <span className="day-count">
                                    {isBlocked ? '🚫' : availableCount}
                                </span>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Legend */}
            <div className="calendar-legend">
                <div className="legend-item">
                    <span className="legend-color available"></span>
                    <span>{t('availableRoomsCount')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color discounted"></span>
                    <span>{t('withDiscount')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color full"></span>
                    <span>{t('full')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color blocked"></span>
                    <span>{t('blocked')}</span>
                </div>
            </div>

            {/* Bulk Edit Panel */}
            {showBulkEdit && selectionStart && selectionEnd && (
                <div className="bulk-edit-panel">
                    <h4>
                        {t('editing')}: {selectionStart.toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')} - {selectionEnd.toLocaleDateString(language === 'bg' ? 'bg-BG' : 'en-US')}
                    </h4>

                    <div className="bulk-edit-form">
                        <div className="form-group">
                            <label>{t('numberOfAvailableRooms')}</label>
                            <div className="count-selector">
                                <button
                                    onClick={() => setBulkEditCount(Math.max(0, bulkEditCount - 1))}
                                    disabled={bulkEditCount <= 0}
                                >
                                    −
                                </button>
                                <input
                                    type="number"
                                    value={bulkEditCount}
                                    onChange={(e) => setBulkEditCount(Math.max(0, parseInt(e.target.value) || 0))}
                                    min="0"
                                    max={selectedRoomType?.totalRooms || 100}
                                />
                                <button
                                    onClick={() => setBulkEditCount(bulkEditCount + 1)}
                                    disabled={bulkEditCount >= (selectedRoomType?.totalRooms || 100)}
                                >
                                    +
                                </button>
                            </div>
                            <small>{t('maximum')}: {selectedRoomType?.totalRooms} {t('rooms')}</small>
                        </div>

                        <div className="form-group">
                            <label>{t('discountPercentage')}:</label>
                            <div className="count-selector">
                                <button
                                    onClick={() => setDiscountPercentage(Math.max(0, discountPercentage - 5))}
                                    disabled={discountPercentage <= 0}
                                >
                                    −
                                </button>
                                <input
                                    type="number"
                                    value={discountPercentage}
                                    onChange={(e) => setDiscountPercentage(Math.min(99, Math.max(0, parseInt(e.target.value) || 0)))}
                                    min="0"
                                    max="99"
                                />
                                <button
                                    onClick={() => setDiscountPercentage(Math.min(99, discountPercentage + 5))}
                                    disabled={discountPercentage >= 99}
                                >
                                    +
                                </button>
                            </div>
                            <small>{t('discountHint')}</small>
                        </div>

                        <div className="bulk-actions">
                            <button className="btn-update" onClick={handleBulkUpdate}>
                                ✓ {t('save')}
                            </button>
                            <button className="btn-block" onClick={() => handleBlockDates(true)}>
                                🚫 {t('block')}
                            </button>
                            <button className="btn-unblock" onClick={() => handleBlockDates(false)}>
                                ✓ {t('unblock')}
                            </button>
                            <button className="btn-cancel" onClick={() => {
                                setShowBulkEdit(false);
                                setSelectionStart(null);
                                setSelectionEnd(null);
                            }}>
                                {t('cancel')}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="calendar-instructions">
                <p>💡 {t('calendarClickHint')}</p>
            </div>
        </div>
    );
};

export default AvailabilityCalendar;
