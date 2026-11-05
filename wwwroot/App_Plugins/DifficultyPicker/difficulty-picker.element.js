import { LitElement, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

// Samma värden som Models/DifficultyValues.cs
const DIFFICULTY_VALUES = ['easy', 'medium', 'hard'];

/**
 * Property Editor UI för datatypen "Difficulty": en dropdown med easy, medium och hard, sparad som en sträng.
 * Det tomma valet gör att en led kan sakna svårighet. Ett värde som inte är ett av de tre visas som tomt.
 */
export class DifficultyPickerElement extends UmbElementMixin(LitElement) {
    static properties = {
        value: { type: String },
        readonly: { type: Boolean, reflect: true },
    };

    #onChange(event) {
        this.value = event.target.value;
        this.dispatchEvent(new UmbChangeEvent());
    }

    render() {
        const selected = DIFFICULTY_VALUES.includes(this.value) ? this.value : '';
        const options = [
            { name: '', value: '', selected: selected === '' },
            ...DIFFICULTY_VALUES.map((value) => ({ name: value, value, selected: value === selected })),
        ];

        return html`<uui-select
            label="Difficulty"
            .options=${options}
            .value=${selected}
            ?readonly=${this.readonly}
            @change=${this.#onChange}></uui-select>`;
    }

    static styles = css`
        :host {
            display: block;
        }
    `;
}

customElements.define('outdoorguide-difficulty-picker', DifficultyPickerElement);

export default DifficultyPickerElement;
