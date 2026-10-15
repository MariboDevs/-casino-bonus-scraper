import { ChangeDetectionStrategy, Component } from '@angular/core';

interface MethodStep {
  readonly n: string;
  readonly icon: string;
  readonly title: string;
  readonly body: string;
}

/** "How each figure is checked": a static explanation of the pipeline. */
@Component({
  selector: 'app-method-steps',
  templateUrl: './method-steps.html',
  styleUrl: './method-steps.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MethodSteps {
  protected readonly steps: readonly MethodStep[] = [
    {
      n: '01',
      icon: 'travel_explore',
      title: 'Polite fetch',
      body: 'Each review page is checked against robots.txt and fetched at least 2 seconds apart. A refusal, 403 or bot check ends the attempt; it is never bypassed.',
    },
    {
      n: '02',
      icon: 'data_object',
      title: 'Structured reading',
      body: 'A language model reads the bonus terms into one strict JSON schema. It is told to answer null rather than guess.',
    },
    {
      n: '03',
      icon: 'fact_check',
      title: 'Deterministic cross-check',
      body: "Every figure the model returns must appear in the source text, read from the operator's terms box where the page has one.",
    },
    {
      n: '04',
      icon: 'publish',
      title: 'Published daily',
      body: 'Offers that pass are marked Checked. Anything else is published as Needs review, with the reason.',
    },
  ];
}
