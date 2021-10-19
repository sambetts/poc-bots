import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import botImg from '../../img/robotbig.png';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <h1>Welcome</h1>
                <p>This is where BastardBot lives. Talk to it (take the abuse), teach it new insults; BastardBot is here for all your insulting needs.</p>
                <img src={botImg} alt="Robot logo" />
                <p>
                    <span style={{ fontWeight: "600", color: "darkred" }}>Mature content: </span>
                    <span style={{ fontWeight: "600" }}>not for kids or the easily offended</span>. By using this site, you agree you're neither.
                </p>
                
                <p><Link to="/chat">Chat to BastardBot</Link> - BastardBot is waiting for your abuse!</p>

                <h2>FAQ</h2>
                <p>Why is this a thing? Swearing and insults can be fun. If you're not convinced by that argument, you're probably best somewhere else.</p>
                <p>Also, it's to some extent a chance for me, the nameless author, to learn some AI &amp; some React.</p>
                <p>
                    <span style={{ fontWeight: "600" }}>Important</span>: This system is pretty open in terms of what you can teach BastardBot.
                    But I reserve the right to clean up some insults if they're too much - this isn't an exercise in free speech; this is just a casual project to get some lulz for the right crowd.</p>
            </div>

        );
    }
}
