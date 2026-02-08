import React, { useState, useEffect } from 'react';
import './DateRangeCalendar.css';

const DateRangeCalendar = ({
    hotelId,
    selectedRoomType,
    checkInDate,
    checkOutDate,
    onDateChange,
    availability = []
}) => {
    const [currentMonth, setCurrentMonth] = useState(new Date());
    const [selectionMode, setSelectionMode] = useState('checkIn'); // 'checkIn' or 'checkOut'

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const getAvailabilityForDate = (date) => {
        if (!selectedRoomType) return null;
        const dateStr = date.toISOString().split('T')[0];
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

        const dateStr = date.toISOString().split('T')[0];

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
        const months = ['Януари', 'Февруари', 'Март', 'Април', 'Май', 'Юни',
            'Юли', 'Август', 'Септември', 'Октомври', 'Ноември', 'Декември'];
        return `${months[date.getMonth()]} ${date.getFullYear()}`;
    };

    const formatDate = (dateStr) => {
        if (!dateStr) return '—';
        const date = new Date(dateStr);
        return date.toLocaleDateString('bg-BG', { day: 'numeric', month: 'short' });
    };

    return (
        <div className="date-range-calendar">
            {/* Selected Dates Summary */}
            <div className="selected-dates-summary">
                <div
                    className={`date-box ${selectionMode === 'checkIn' ? 'active' : ''} ${checkInDate ? 'filled' : ''}`}
                    onClick={() => setSelectionMode('checkIn')}
                >
                    <span className="date-label">Настаняване</span>
                    <span className="date-value">{formatDate(checkInDate)}</span>
                </div>
                <div className="date-separator">→</div>
                <div
                    className={`date-box ${selectionMode === 'checkOut' ? 'active' : ''} ${checkOutDate ? 'filled' : ''}`}
                    onClick={() => setSelectionMode('checkOut')}
                >
                    <span className="date-label">Напускане</span>
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
                <div className="day-header">Нд</div>
                <div className="day-header">Пн</div>
                <div className="day-header">Вт</div>
                <div className="day-header">Ср</div>
                <div className="day-header">Чт</div>
                <div className="day-header">Пт</div>
                <div className="day-header">Сб</div>

                {getDaysInMonth().map((date, index) => {
                    if (!date) {
                        return <div key={index} className="day-cell empty"></div>;
                    }

                    const dateStr = date.toISOString().split('T')[0];
                    const isPast = date < today;
                    const isBlocked = isDateBlocked(date);
                    const isCheckIn = checkInDate && dateStr === checkInDate;
                    const isCheckOut = checkOutDate && dateStr === checkOutDate;
                    const isInRange = isDateInRange(date);
                    const avail = getAvailabilityForDate(date);
                    const availableCount = avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0;
                    const hasDiscount = avail?.discountPercentage > 0;

                    return (
                        <div
                            key={index}
                            className={`day-cell 
                                ${isPast ? 'past' : ''} 
                                ${isBlocked ? 'blocked' : ''}
                                ${isCheckIn ? 'check-in' : ''}
                                ${isCheckOut ? 'check-out' : ''}
                                ${isInRange ? 'in-range' : ''}
                                ${!isPast && !isBlocked ? 'available' : ''}
                                ${hasDiscount && !isPast && !isBlocked ? 'discounted' : ''}
                            `}
                            onClick={() => handleDayClick(date)}
                        >
                            {hasDiscount && !isPast && !isBlocked && (
                                <span className="discount-badge">-{avail.discountPercentage}%</span>
                            )}
                            <span className="day-number">{date.getDate()}</span>
                            {selectedRoomType && !isPast && !isBlocked && (
                                <span className="availability-indicator">
                                    {availableCount > 0 ? `${availableCount}` : ''}
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
                    <span>Налично</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot discounted"></span>
                    <span>С отстъпка</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot selected"></span>
                    <span>Избрано</span>
                </div>
                <div className="legend-item">
                    <span className="legend-dot blocked"></span>
                    <span>Заето</span>
                </div>
            </div>

            {/* Instructions */}
            <p className="calendar-hint">
                {!checkInDate
                    ? '👆 Изберете дата на настаняване'
                    : !checkOutDate
                        ? '👆 Изберете дата на напускане'
                        : '✓ Датите са избрани'
                }
            </p>
        </div>
    );
};

export default DateRangeCalendar;
