import { ToggleSwitch, SelectElement, Variant } from '@appology/react-components';
import IBaseModel from '@appology/react-components/dist/SelectionRefinement/IBaseModel';
import * as React from 'react';
import Select, { ValueType } from 'react-select';
import { api } from 'src/Api/Api';
import { ActionButton } from 'src/components/utils/ActionButtons';
import { ICalendarSettings } from 'src/models/IUser';
import { ISelect } from 'src/models/ISelect';
import IUserType from '@appology/react-components/dist/UserTypes/IUserType';

export interface IOwnProps {
    settings: ICalendarSettings,
    userCalendars: IUserType[],
    showAlert: (txt: string, variant: Variant) => void
}

export interface IOwnState {
    settings: ICalendarSettings,
    save: boolean
}

export class CalendarGeneral extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            settings: this.props.settings,
            save: false
        };
    }

    public render() {

        const views : IBaseModel[] = [
            {id: "dayGridMonth", name: "Month"},
            {id: "dayGridWeek", name: "Week"},
            {id: "dayGrid", name: "Day"},
            {id: "listWeek", name: "Agenda"}
        ]

        

        return (
            <form onSubmit={this.handleSubmit}>
                <ToggleSwitch id="EnableCronofy" name="Enable Appology Calendar sync" checked={this.state.settings.enableCronofy} onChange={this.enableCronofyChange} />
                <hr />
                <div style={{ marginBottom: 20 }}>
                  <label>Default Selection</label>
                  <Select 
                      options={this.props.userCalendars
                        .map(o => ({
                            value: o.key.toString(), 
                            label: `${o.title} ${(o.creatorName ? `(by ${o.creatorName})` : "")}`}
                        ))}
                      placeholder="Select Calendars"
                      isMulti={true}
                      defaultValue={this.props.userCalendars.filter(uc => uc.selected).map(uc => ({ value: uc.key.toString(), label:  `${uc.title} ${(uc.creatorName ? `(by ${uc.creatorName})` : "")}`}))}
                      onChange={ (value: ValueType<ISelect, true>)  => this.handleCalendarsChange(value)}
                  />
                </div>
                <SelectElement 
                    label="Default View (for Desktop)"
                    id="DefaultCalendarView"
                    icon="&#xf292;"
                    selected={this.state.settings.defaultCalendarView}
                    selectorOptions={views}
                    onSelectChange={this.defaultCalendarViewChange} 
                />
                <SelectElement 
                    label="Default View (for Mobile)"
                    id="DefaultNativeCalendarView"
                    icon="&#xf2c8;"
                    selected={this.state.settings.defaultNativeCalendarView}
                    selectorOptions={views}
                    onSelectChange={this.defaultNativeCalendarViewChange} 
                />
                <div>
                    <ActionButton loading={this.state.save} />
                </div>
            </form>
        );
    }

    private handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        this.setState({ save: true })

        api.saveUserCalendarSettings(this.state.settings)
            .then(status => this.saveCalendarSettingsSuccess(status));
    }

    private saveCalendarSettingsSuccess = (status: boolean) => {
        this.setState({ save: false })

        if (status) {
            this.props.showAlert("Calendar settings saved successfully", Variant.Success)
        }
        else{
            this.props.showAlert("There was an issue with updating calendar settings", Variant.Danger)
        }
    }

    private handleCalendarsChange = (calendars: ValueType<ISelect, true>) => {

        if (calendars.length === 0) {
            this.props.showAlert("Must have at least one selected calendar", Variant.Danger)
            return;
        }

        this.setState({
            settings: { ...this.state.settings,    
                selectedCalendars: calendars.map(c => c.value).join(',')
            } 
        })
    }

    private defaultCalendarViewChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({
            settings: { ...this.state.settings,    
                defaultCalendarView: e.target.value
            } 
        })
    }

    private defaultNativeCalendarViewChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({
            settings: { ...this.state.settings,    
                defaultNativeCalendarView: e.target.value
            } 
        })
    }


    private enableCronofyChange = (checked: boolean) => {
        this.setState({
            settings: { ...this.state.settings,    
                enableCronofy: checked   
            }
        })
    }
}