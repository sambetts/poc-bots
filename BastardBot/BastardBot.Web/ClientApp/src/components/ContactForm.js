import React from 'react';
import ReCAPTCHA from "react-google-recaptcha";


export class ContactForm extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            sent: false,
            name: '',
            email: '',
            message: '',
            recaptchaValue: ''
        }
    }

    render() {
        if (this.state.sent) {
            return (
                <div>Email sent!</div>
            );
        }
        else {
            return (
                <div>
                    <ReCAPTCHA sitekey="6Le2gvAUAAAAAJmUqssFXdGd3v70blKNJjdFafRl"
                        onChange={this.onChange.bind(this)} />

                    <form id="contact-form" onSubmit={this.handleSubmit.bind(this)} method="POST">
                        <div className="form-group">
                            <label htmlFor="name">Name</label>
                            <input type="text" className="form-control" onChange={this.onNameChange.bind(this)} required />
                        </div>
                        <div className="form-group">
                            <label htmlFor="exampleInputEmail1">Email address</label>
                            <input type="email" className="form-control" aria-describedby="emailHelp" onChange={this.onEmailChange.bind(this)} required />
                        </div>
                        <div className="form-group">
                            <label htmlFor="message">Message</label>
                            <textarea className="form-control" rows="5" onChange={this.onMessageChange.bind(this)} required></textarea>
                        </div>
                        <button disabled={!this.state.recaptchaValue} type="submit" className="btn btn-primary">Submit</button>
                    </form>
                </div>
            );
        }
    }

    onChange(value) {
        this.setState({ recaptchaValue: value});
        console.log("Captcha value:", value);
    }

    resetForm() {
        this.setState({ sent: true, name: '', email: '', message: '' })
    }

    onNameChange(event) {
        this.setState({ name: event.target.value })
    }

    onEmailChange(event) {
        this.setState({ email: event.target.value })
    }

    onMessageChange(event) {
        this.setState({ message: event.target.value })
    }
    handleSubmit(e) {
        e.preventDefault();

        fetch('/contactus', {
            method: "POST",
            body: JSON.stringify(this.state),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
        }).then(
            (response) => {
                console.log(response);
                if (response.ok) {
                    response.json()
                        .then((response) => {
                            if (response.status === 'success') {
                                alert("Message Sent.");
                                this.resetForm();
                            }
                            else {
                                alert("Message failed to send: " + response.status);
                            }
                        })
                }
                else {
                    alert('Unexpected error sending email!');
                }
            })
            .catch((err) => {
                alert("Couldn't send the email: " + err.message);
            })
    }

}