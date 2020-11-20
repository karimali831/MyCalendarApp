import * as React from 'react';
import './ToggleSwitch.css';

interface IOwnProps {
    id: string
    name: string;
    onText?: string;
    offText?: string;
    checked: boolean;
    disabled?: false;
    onChange: (checked: boolean) => void
}

export const ToggleSwitch: React.FC<IOwnProps> = (props) => {
    return (
        <div>
          <span style={{ marginRight: 10 }}>{props.name}</span>
          <div className="toggle-switch small-switch">
            <input
              type="checkbox"
              className="toggle-switch-checkbox"
              id={props.id}
              name={props.name}
              checked={props.checked}
              onChange={e => props.onChange(e.target.checked)}
              disabled={props.disabled}
            />
            {props.id ? (
              <label className="toggle-switch-label" htmlFor={props.id}>
                <span
                  className={
                    props.disabled
                      ? "toggle-switch-inner toggle-switch-disabled"
                      : "toggle-switch-inner"
                  }
                  data-yes={props.onText !== null ? props.onText : "On"}
                  data-no={props.offText !== null ? props.offText : "Off"}
                />
                <span
                  className={
                  props.disabled
                    ? "toggle-switch-switch toggle-switch-disabled"
                    : "toggle-switch-switch"
                  }
                />
              </label>
            ) : null}
          </div>
        </div>
      );
}