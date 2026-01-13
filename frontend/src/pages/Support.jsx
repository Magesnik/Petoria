import React, { useState } from 'react';
import { useLanguage } from '../context/LanguageContext';
import Header from '../components/Header';
import './Support.css';

const Support = () => {
    const { t } = useLanguage();
    const [formData, setFormData] = useState({
        subject: '',
        message: '',
    });
    const [submitMessage, setSubmitMessage] = useState('');
    const [openFaq, setOpenFaq] = useState(null);

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        // TODO: Add API call to send support message
        setSubmitMessage(t('messageSent'));
        setFormData({
            subject: '',
            message: '',
        });
        setTimeout(() => setSubmitMessage(''), 5000);
    };

    const toggleFaq = (index) => {
        setOpenFaq(openFaq === index ? null : index);
    };

    const faqs = [
        {
            question: 'How do I make a booking?',
            answer: 'Browse our hotels, select your preferred accommodation, choose dates, and click "Book Now". Follow the checkout process to complete your reservation.',
        },
        {
            question: 'Can I cancel my booking?',
            answer: 'Cancellation policies vary by hotel. Check the specific hotel\'s cancellation policy before booking. You can manage your bookings from the Purchase History page.',
        },
        {
            question: 'How do I change my account settings?',
            answer: 'Click on your name in the header, then select "Settings" from the dropdown menu. You can update your profile information and change your password there.',
        },
        {
            question: 'What payment methods do you accept?',
            answer: 'We accept all major credit cards (Visa, MasterCard, American Express) and PayPal for secure online payments.',
        },
        {
            question: 'How can I contact a hotel directly?',
            answer: 'Visit the hotel details page and scroll down to find the contact information section with phone number and email address.',
        },
    ];

    return (
        <>
            <Header />
            <div className="support-page">
                <div className="support-hero">
                    <h1>{t('supportTitle')}</h1>
                    <p>{t('supportSubtitle')}</p>
                </div>

                <div className="support-container">
                    {/* Contact Form */}
                    <div className="support-section">
                        <h2>{t('contactSupportForm')}</h2>
                        <form onSubmit={handleSubmit} className="support-form">
                            <div className="form-group">
                                <label htmlFor="subject">{t('subject')}</label>
                                <input
                                    type="text"
                                    id="subject"
                                    name="subject"
                                    value={formData.subject}
                                    onChange={handleChange}
                                    required
                                    placeholder="What can we help you with?"
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="message">{t('yourMessage')}</label>
                                <textarea
                                    id="message"
                                    name="message"
                                    value={formData.message}
                                    onChange={handleChange}
                                    required
                                    rows="6"
                                    placeholder="Please describe your question or issue in detail..."
                                />
                            </div>
                            {submitMessage && <div className="success-message">{submitMessage}</div>}
                            <button type="submit" className="btn btn-primary">
                                {t('sendMessage')}
                            </button>
                        </form>
                    </div>

                    {/* FAQ Section */}
                    <div className="support-section">
                        <h2>{t('faqTitle')}</h2>
                        <div className="faq-list">
                            {faqs.map((faq, index) => (
                                <div key={index} className={`faq-item ${openFaq === index ? 'open' : ''}`}>
                                    <button className="faq-question" onClick={() => toggleFaq(index)}>
                                        <span>{faq.question}</span>
                                        <span className="faq-icon">{openFaq === index ? '−' : '+'}</span>
                                    </button>
                                    {openFaq === index && (
                                        <div className="faq-answer">
                                            <p>{faq.answer}</p>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Contact Info */}
                    <div className="contact-cards">
                        <div className="contact-card">
                            <div className="contact-icon">📧</div>
                            <h3>Email</h3>
                            <p>support@petoria.com</p>
                        </div>
                        <div className="contact-card">
                            <div className="contact-icon">📞</div>
                            <h3>Phone</h3>
                            <p>+1 (555) 123-4567</p>
                        </div>
                        <div className="contact-card">
                            <div className="contact-icon">💬</div>
                            <h3>Live Chat</h3>
                            <p>Available 24/7</p>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default Support;
