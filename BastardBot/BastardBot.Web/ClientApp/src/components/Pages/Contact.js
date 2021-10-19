import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { ContactForm } from '../ContactForm';

export class Contact extends Component {

    render() {
        return (
            <div>
                <h1>Contact</h1>
                <p>If you really feel the need to contact me, you can below.</p>
                <ContactForm />
            </div>

        );
    }
}
