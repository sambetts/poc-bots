import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Pages/Home';
import { Chat } from './components/Pages/Chat';
import { Contact } from './components/Pages/Contact';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route path='/chat' component={Chat} />
                <Route path='/contact' component={Contact} />
            </Layout>
        );
    }
}
