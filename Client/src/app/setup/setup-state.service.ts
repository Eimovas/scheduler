import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { deepCopy } from '../shared/deep-copy';
import { SetupFormService } from './setup-form.service';
import { SetupDTO } from './setup.models';

@Injectable({
    providedIn: 'root'
})
export class SetupStateService {
    private setupFormSubject = new BehaviorSubject<SetupDTO>(null);
    public setupForm$ = this.setupFormSubject.asObservable();

    constructor(setupFormService: SetupFormService) {
        setupFormService.setupForm.valueChanges.subscribe(form => this.onChange(form));
        // TODO: load some existing one if exist.
    }

    public onChange(setup: SetupDTO): void {
        const copy = deepCopy(setup);
        this.setupFormSubject.next(copy);
        console.log(copy);
        
    }
}
