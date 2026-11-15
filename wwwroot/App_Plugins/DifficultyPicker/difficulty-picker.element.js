import { LitElement, css, html, svg, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

// Samma värden som Models/DifficultyValues.cs
const DIFFICULTY_VALUES = ['easy', 'medium', 'hard'];

/**
 * Property Editor UI för datatypen "Difficulty" (OUT-76, OUT-81, OUT-91).
 * Tre kort, Lätt, Medel och Svår, med en kort förklaring och en ikon med 1–3 staplar, så att nivåerna syns även utan färg.
 * Sparar samma sträng som förut: easy, medium eller hard. "Ta bort valet" gör att en led kan sakna svårighet.
 * Riktiga radioknappar i shadow DOM, så Tab och piltangenter fungerar som vanligt.
 */
// Antal fyllda staplar i ikonen, så att nivåerna skiljer sig åt även utan färg
const LEVELS = { easy: 1, medium: 2, hard: 3 };

export class DifficultyPickerElement extends UmbElementMixin(LitElement) {
	static properties = {
		value: { type: String },
		readonly: { type: Boolean, reflect: true },
	};

	constructor() {
		super();
		this.value = '';
		this.readonly = false;
	}

	#label(item) {
		return this.localize.term(`outdoorGuideDifficulty_${item}`);
	}

	#hint(item) {
		return this.localize.term(`outdoorGuideDifficulty_${item}Hint`);
	}

	// Ett värde som inte är ett av de tre visas som inget val, som i den tidigare dropdownen
	get #selected() {
		return DIFFICULTY_VALUES.includes(this.value) ? this.value : '';
	}

	#select(item) {
		if (this.readonly || this.value === item) {
			return;
		}

		this.value = item;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#clear() {
		if (this.readonly || !this.#selected) {
			return;
		}

		this.value = '';
		this.dispatchEvent(new UmbChangeEvent());
	}

	#bars(item) {
		const filled = LEVELS[item] ?? 0;

		return html`<svg class="bars" viewBox="0 0 24 24" aria-hidden="true">
			${[0, 1, 2].map(
				// svg`` och inte html``, annars hamnar rect utanför SVG:ns namnrymd och syns inte
				(i) => svg`<rect
					x=${3 + i * 7}
					y=${14 - i * 5}
					width="4"
					height=${6 + i * 5}
					rx="1"
					class=${i < filled ? 'on' : 'off'}></rect>`,
			)}
		</svg>`;
	}

	render() {
		const name = 'og-difficulty';

		return html`
			<fieldset ?disabled=${this.readonly}>
				<legend class="visually-hidden">${this.localize.term('outdoorGuideDifficulty_label')}</legend>
				<div class="options">
					${DIFFICULTY_VALUES.map(
						(item) => html`
							<label class="option level-${item}">
								<input
									type="radio"
									name=${name}
									.value=${item}
									.checked=${this.#selected === item}
									@change=${() => this.#select(item)} />
								<span class="card">
									${this.#bars(item)}
									<span class="text">
										<strong>${this.#label(item)}</strong>
										<span class="hint">${this.#hint(item)}</span>
									</span>
								</span>
							</label>
						`,
					)}
				</div>
			</fieldset>
			${this.#selected && !this.readonly
				? html`<button type="button" class="clear" @click=${this.#clear}>
						${this.localize.term('outdoorGuideDifficulty_clear')}
					</button>`
				: nothing}
			${!this.#selected
				? html`<p class="none">${this.localize.term('outdoorGuideDifficulty_none')}</p>`
				: nothing}
		`;
	}

	static styles = css`
		:host {
			display: block;
			/* Sajtens färger, så att väljaren känns igen från ledsidan */
			--og-forest: #1f4d3a;
			--og-moss: #eef3ef;
			--og-border: #b9b3a4;
			--og-muted: #4f5b53;
			--og-easy: #2e6650;
			--og-medium: #a2441a;
			--og-hard: #7a1f1f;
		}

		fieldset {
			margin: 0;
			padding: 0;
			border: 0;
			min-width: 0;
		}

		.visually-hidden {
			position: absolute;
			width: 1px;
			height: 1px;
			overflow: hidden;
			clip: rect(0 0 0 0);
			white-space: nowrap;
		}

		.options {
			display: grid;
			grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
			gap: var(--uui-size-space-3, 9px);
			max-width: 640px;
		}

		.option {
			position: relative;
			display: block;
			cursor: pointer;
		}

		/* Radioknappen ligger kvar för tangentbordet och skärmläsare, men det är kortet man ser */
		.option input {
			position: absolute;
			inset: 0;
			margin: 0;
			opacity: 0;
			cursor: inherit;
		}

		.card {
			display: flex;
			align-items: center;
			gap: 10px;
			height: 100%;
			box-sizing: border-box;
			padding: 10px 12px;
			border: 1px solid var(--og-border);
			border-radius: var(--uui-border-radius, 6px);
			background: var(--uui-color-surface, #fff);
			color: var(--uui-color-text, #1c2a22);
			transition: border-color 0.15s ease, background-color 0.15s ease;
		}

		.option:hover .card {
			border-color: var(--og-forest);
		}

		.option input:checked + .card {
			border: 2px solid var(--og-forest);
			padding: 9px 11px; /* samma storlek trots tjockare kant */
			background: var(--og-moss);
		}

		.option input:focus-visible + .card {
			outline: 2px solid var(--uui-color-focus, #3879ff);
			outline-offset: 2px;
		}

		.text {
			display: flex;
			flex-direction: column;
			line-height: 1.3;
		}

		.hint {
			font-size: var(--uui-type-small-size, 12px);
			color: var(--og-muted);
		}

		.bars {
			flex: none;
			width: 24px;
			height: 24px;
		}

		.bars .off {
			fill: #d9d4c7;
		}

		.level-easy .bars .on {
			fill: var(--og-easy);
		}

		.level-medium .bars .on {
			fill: var(--og-medium);
		}

		.level-hard .bars .on {
			fill: var(--og-hard);
		}

		.clear {
			margin-top: var(--uui-size-space-2, 6px);
			padding: 0;
			border: 0;
			background: none;
			color: var(--uui-color-interactive, var(--og-forest));
			font: inherit;
			text-decoration: underline;
			cursor: pointer;
		}

		.none {
			margin: var(--uui-size-space-2, 6px) 0 0;
			font-size: var(--uui-type-small-size, 12px);
			color: var(--og-muted);
		}

		:host([readonly]) .option {
			cursor: default;
		}

		:host([readonly]) .card {
			opacity: 0.75;
		}

		@media (prefers-reduced-motion: reduce) {
			.card {
				transition: none;
			}
		}
	`;
}

customElements.define('outdoorguide-difficulty-picker', DifficultyPickerElement);

export default DifficultyPickerElement;
