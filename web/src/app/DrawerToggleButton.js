import React from 'react';
import { withStyles } from '@material-ui/core/styles';

import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';

const styles = {
    menuButton: {
        marginLeft: -12,
        marginRight: 20,
    },
};

const DrawerToggleButton = (props) => {
    return (
        <div style={styles.container}>
            <IconButton 
                className={props.classes.menuButton} 
                color="inherit" 
                onClick={props.onToggleClick}
            >
                <MenuIcon/>
            </IconButton>
        </div>
    );
}

export default withStyles(styles)(DrawerToggleButton);