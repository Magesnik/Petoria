import React, { useState } from 'react';
import { useLanguage } from '../../context/LanguageContext';
import './DateRangeCalendar.css';

const DateRangeCalendar = ({
    selectedRoomType,
    checkInDate,
    checkOutDate,
    onDateChange,
    availability = []
}) => {
    const { t, language } = useLanguage();
    const [currentMonth, setCurrentMonth] = useState(new Date());
    const [selectionMode, setSelectionMode] = useState('checkIn'); // 'checkIn' or 'checkOut'

    // Use local date string to avoid UTC timezone shifting (e.g. UTC+2 shifts dates back 1 day)
    const toLocalDateStr = (date) => {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    };

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const getAvailabilityForDate = (date) => {
        if (!selectedRoomType) return null;
        const dateStr = toLocalDateStr(date);
        return availability.find(
            a => a.roomTypeId === selectedRoomType.id && a.date.split('T')[0] === dateStr
        );
    };

    const isDateBlocked = (date) => {
        const avail = getAvailabilityForDate(date);
        return avail?.isBlocked || avail?.availableCount === 0;
    };

    const isDateInRange = (date) => {
        if (!checkInDate || !checkOutDate) return false;
        const checkIn = new Date(checkInDate);
        const checkOut = new Date(checkOutDate);
        return date > checkIn && date < checkOut;
    };

    const handleDayClick = (date) => {
        if (date < today || isDateBlocked(date)) return;

        const dateStr = toLocalDateStr(date);

        if (selectionMode === 'checkIn') {
            onDateChange(dateStr, null);
            setSelectionMode('checkOut');
        } else {
            // Check-out must be after check-in
            if (checkInDate && new Date(dateStr) > new Date(checkInDate)) {
                onDateChange(checkInDate, dateStr);
                setSelectionMode('checkIn');
            } else {
                // If clicked date is before check-in, start over
                onDateChange(dateStr, null);
                setSelectionMode('checkOut');
            }
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

        // Add empty days for padding (start week on Sunday)
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

    const formatDate = (dateStr) => {
        if (!dateStr) return '—';
        // Parse as local date to avoid UTC shifting
        const [year, month, day] = dateStr.split('-').map(Number);
        const date = new Date(year, month - 1, day);
        const locale = language === 'bg' ? 'bg-BG' : 'en-US';
        return date.toLocaleDateString(locale, { day: 'numeric', month: 'short' });
    };

    return (
        <div className="date-range-calendar">
            {/* Selected Dates Summary */}
            <div className="selected-dates-summary">
                <div
                    className={`date-box ${selectionMode === 'checkIn' ? 'active' : ''} ${checkInDate ? 'filled' : ''}`}
                    onClick={() => setSelectionMode('checkIn')}
                >
                    <span className="date-label">{t('checkIn')}</span>
                    <span className="date-value">{formatDate(checkInDate)}</span>
                </div>
                <div className="date-separator">→</div>
                <div
                    className={`date-box ${selectionMode === 'checkOut' ? 'active' : ''} ${checkOutDate ? 'filled' : ''}`}
                    onClick={() => setSelectionMode('checkOut')}
                >
                    <span className="date-label">{t('checkOut')}</span>
                    <span className="date-value">{formatDate(checkOutDate)}</span>
                </div>
            </div>

            {/* Calendar Navigation */}
            <div className="calendar-nav">
                <button onClick={previousMonth} className="nav-btn">‹</button>
                <span className="current-month">{formatMonth(currentMonth)}</span>
                <button onClick={nextMonth} className="nav-btn">›</button>
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

                    const dateStr = toLocalDateStr(date);
                    const isPast = date < today;

                    const isCheckIn = checkInDate && dateStr === checkInDate;
                    const isCheckOut = checkOutDate && dateStr === checkOutDate;
                    const isInRange = isDateInRange(date);
                    const avail = getAvailabilityForDate(date);
                    const availableCount = avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0;

                    // A date is structurally blocked from selection if it's explicitly blocked OR has 0 rooms.
                    const isZeroAvailable = availableCount === 0;
                    const isExplicitlyBlocked = avail?.isBlocked;
                    const isBlockedForSelection = isExplicitlyBlocked || isZeroAvailable;

                    const hasDiscount = avail?.discountPercentage > 0;

                    return (
                        <div
                            key={index}
                            className={`day-cell 
                                ${isPast ? 'past' : ''} 
                                ${isExplicitlyBlocked ? 'blocked' : ''}
                                ${isZeroAvailable && !isExplicitlyBlocked && !isPast ? 'full blocked' : ''}
                                ${isCheckIn ? 'check-in' : ''}
                                ${isCheckOut ? 'check-out' : ''}
                                ${isInRange ? 'in-range' : ''}
                                ${!isPast && !isExplicitlyBlocked && availableCount > 0 ? 'available' : ''}
                                ${hasDiscount && !isPast && !isExplicitlyBlocked && availableCount > 0 ? 'discounted' : ''}
                            `}
                            onClick={() => !isBlockedForSelection && handleDayClick(date)}
                        >
                            {hasDiscount && !isPast && !isExplicitlyBlocked && (
                                <span className="discount-badge">-{avail.discountPercentage}%</span>
                            )}
                            <span className="day-number">{date.getDate()}</span>
                            {selectedRoomType && !isPast && !isExplicitlyBlocked && (
                                <span className={`availability-indicator ${isZeroAvailable ? 'full' : ''}`}>
                                    {!isZeroAvailable ? `${availableCount}` : t('legendBlocked')}
                                </span>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Legend */}
            <div className="calendar-legend">
                <div className="legend-item">
                    <span className="legend-dot available"></span>
                    <span>{t('legendAvailable')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot discounted"></span>
                    <span>{t('legendDiscounted')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot selected"></span>
                    <span>{t('legendSelected')}</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot blocked"></span>
                    <span>{t('legendBlocked')}</span>
                </div>
            </div>

            {/* Instructions */}
            <p className="calendar-hint">
                {!checkInDate
                    ? `👆 ${t('selectCheckInDate')}`
                    : !checkOutDate
                        ? `👆 ${t('selectCheckOutDate')}`
                        : `✓ ${t('datesSelected')}`
                }
            </p>
        </div>
    );
};

export default DateRangeCalendar;
