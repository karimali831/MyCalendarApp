import * as React from 'react';
import './ToggleSwitch.css';

interface IOwnProps {
    name: string;
    onText?: string;
    offText?: string;
}

export const ToggleSwitch: React.FC<IOwnProps> = (props) => {
    return (
        <div className="toggle-switch small-switch">
          <input
            type="checkbox"
            className="toggle-switch-checkbox"
            name={props.name}
            id={props.name}
          />
          <label className="toggle-switch-label" htmlFor={props.name}>
            <span className="toggle-switch-inner" data-yes={props.onText !== null ? props.onText : "On"} data-no={props.offText !== null ? props.offText : "Off"} />
            <span className="toggle-switch-switch" />
          </label>
        </div>
      );
}